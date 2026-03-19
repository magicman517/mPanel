using mPanel.API.Core.Entities;
using mPanel.API.Core.Interfaces;

namespace mPanel.API.Infrastructure.Services;

internal sealed class NodeApiService(ILogger<NodeApiService> logger, HttpClient httpClient) : INodeApiService
{
    public async Task<(bool IsSuccess, string? ErrorMessage)> CheckHealthAsync(Node node,
        CancellationToken ct = default)
    {
        var scheme = node.Scheme.ToString().ToLower();
        var url = $"{scheme}://{node.Address}:{node.Port}/health";

        try
        {
            logger.LogInformation("Pinging node '{Name}'", node.Name);

            var response = await httpClient.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            logger.LogWarning("Node '{Name}' health check failed. Status: {StatusCode}", node.Name,
                response.StatusCode);

            var error =
                $"Node responded, but returned an error status: {response.StatusCode} ({(int)response.StatusCode}).";
            return (false, error);
        }
        catch (HttpRequestException e)
        {
            logger.LogWarning("Failed to connect to node '{Name}' at '{Address}': {Message}", node.Name,
                node.Address, e.Message);

            var error =
                $"Failed to connect to {node.Address}:{node.Port}. Ensure the node is running, the port is forwarded, and your firewall allows inbound traffic";
            return (false, error);
        }
        catch (TaskCanceledException e)
        {
            if (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Connection to node '{Name}' timed out: {Message}", node.Name, e.Message);
                return (false, $"Connection timed out. The node at {node.Address} took too long to respond");
            }

            logger.LogWarning("Health check for node '{Name}' was cancelled internally", node.Name);
            return (false, "Health check was aborted internally. Please restart your node");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error checking health for node '{Name}'", node.Name);
            return (false, "An unexpected internal error occurred while trying to reach the node");
        }
    }
}
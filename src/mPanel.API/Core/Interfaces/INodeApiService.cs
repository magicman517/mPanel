using mPanel.API.Core.Entities;

namespace mPanel.API.Core.Interfaces;

internal interface INodeApiService
{
    Task<(bool IsSuccess, string? ErrorMessage)> CheckHealthAsync(Node node, CancellationToken ct = default);
}
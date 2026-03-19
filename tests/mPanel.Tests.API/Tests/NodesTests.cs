using System.Net;
using System.Net.Http.Json;
using mPanel.API.Core.Entities;
using mPanel.API.Core.Enums;
using mPanel.API.Features.Nodes;
using mPanel.API.Features.PanelSettings;

namespace mPanel.Tests.API.Tests;

[Collection(nameof(AspireFixture))]
public class NodesTests(AspireFixture fixture) : TestBase(fixture)
{
    private async Task<CreateNodeResponse> CreateNodeAsync(HttpClient client)
    {
        var settingsRequest = new UpdatePanelSettingsRequest
        {
            Name = "mPanel",
            Url = new Uri("http://localhost:8080"),
            AllowRegistration = true,
            AllowAccountSelfDeletion = true,
            Smtp = new Smtp()
        };
        using var settingsResponse = await client.PutAsJsonAsync("/api/settings", settingsRequest);
        settingsResponse.EnsureSuccessStatusCode();

        var nodeName = $"node-{Guid.NewGuid():N}"[..20];
        var request = new CreateNodeRequest
        {
            Name = nodeName,
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        using var response = await client.PostAsJsonAsync("/api/nodes", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<CreateNodeResponse>();
        Assert.NotNull(content);
        return content;
    }

    [Fact]
    public async Task UpdateNode_Anonymous_ReturnsUnauthorized()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        var request = new UpdateNodeRequest
        {
            Id = Guid.NewGuid(),
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{request.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_UserRole_ReturnsForbidden()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;
        var request = new UpdateNodeRequest
        {
            Id = Guid.NewGuid(),
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{request.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var request = new UpdateNodeRequest
        {
            Id = Guid.NewGuid(),
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{request.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Https,
            Address = Faker.Internet.Ip(),
            Port = 10002,
            Alias = "frankfurt.example.com",
            SftpPort = 2023,
            SftpAlias = "sftp.example.com",
            MaxMemoryMb = 4096,
            MaxDiskMb = 51200,
            IsMaintenanceMode = true,
            IsActive = false
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;

        var firstNode = await CreateNodeAsync(client);

        var secondNodeName = $"node-{Guid.NewGuid():N}"[..20];
        var createRequest = new CreateNodeRequest
        {
            Name = secondNodeName,
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10002,
            SftpPort = 2023,
            IsActive = true
        };
        using var createResponse = await client.PostAsJsonAsync("/api/nodes", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var updateRequest = new UpdateNodeRequest
        {
            Id = firstNode.Id,
            Name = secondNodeName,
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{firstNode.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = "",
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithShortName_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = "ab",
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithLongName_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = new string('a', 129),
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithEmptyAddress_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Http,
            Address = "",
            Port = 10001,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithInvalidPort_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 0,
            SftpPort = 2022,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithSameSftpPort_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 10001,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithZeroMaxMemory_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        // BUG: validator rejects 0 but frontend treats 0 as "no limit"
        // This test documents the current (incorrect) behavior
        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            MaxMemoryMb = 0,
            MaxDiskMb = 0,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNode_WithNullLimits_ReturnsNoContent()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(isAdmin: true);
        using var client = authContext.client;
        var node = await CreateNodeAsync(client);

        var request = new UpdateNodeRequest
        {
            Id = node.Id,
            Name = Faker.Internet.DomainWord(),
            Scheme = NodeConnectionScheme.Http,
            Address = Faker.Internet.Ip(),
            Port = 10001,
            SftpPort = 2022,
            MaxMemoryMb = null,
            MaxDiskMb = null,
            IsActive = true
        };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/nodes/{node.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}

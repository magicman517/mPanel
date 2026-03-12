using System.Net;
using System.Net.Http.Json;
using mPanel.API.Features.ApiKeys;
using mPanel.API.Features.ApiKeys.Shared;

namespace mPanel.Tests.API.Tests;

[Collection(nameof(AspireFixture))]
public class ApiKeysTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task GetApiKeys_Anonymous_ReturnsUnauthorized()
    {
        // Arrange
        using var client = CreateAnonymousClient();

        // Act
        using var response = await client.GetAsync("/api/api-keys");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetApiKeys_Authenticated_ReturnsOkAndEmptyList()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;

        // Act
        using var response = await client.GetAsync("/api/api-keys");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<ApiKeyDto>>();
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task CreateApiKey_Anonymous_ReturnsUnauthorized()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        var request = new CreateApiKeyRequest
        {
            Name = Faker.Name.FullName(),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Act
        using var response = await client.PostAsJsonAsync("/api/api-keys", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateApiKey_Authenticated_ReturnsOkAndApiKeyData()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;
        var request = new CreateApiKeyRequest
        {
            Name = Faker.Name.FullName(),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Act
        using var response = await client.PostAsJsonAsync("/api/api-keys", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        Assert.NotNull(content);
        Assert.False(string.IsNullOrEmpty(content.Key));
    }

    [Fact]
    public async Task CreateApiKey_WithLongName_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;
        var request = new CreateApiKeyRequest
        {
            Name = new string('A', 129),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Act
        using var response = await client.PostAsJsonAsync("/api/api-keys", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateApiKey_WithPastExpiration_ReturnsBadRequest()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;
        var request = new CreateApiKeyRequest
        {
            Name = Faker.Name.FullName(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        using var response = await client.PostAsJsonAsync("/api/api-keys", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteApiKey_Anonymous_ReturnsUnauthorized()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        var apiKeyId = Guid.NewGuid();

        // Act
        using var response = await client.DeleteAsync($"/api/api-keys/{apiKeyId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteApiKey_Authenticated_ReturnsOkAndList()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;
        var createRequest = new CreateApiKeyRequest
        {
            Name = Faker.Name.FullName(),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        using var createResponse = await client.PostAsJsonAsync("/api/api-keys", createRequest);
        var createContent = await createResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        Assert.NotNull(createContent);

        // Act
        using var deleteResponse = await client.DeleteAsync($"/api/api-keys/{createContent.Meta.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var content = await deleteResponse.Content.ReadFromJsonAsync<IEnumerable<ApiKeyDto>>();
        Assert.NotNull(content);
    }
}
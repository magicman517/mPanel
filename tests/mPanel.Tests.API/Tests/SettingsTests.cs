using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using mPanel.API.Features.PanelSettings;

namespace mPanel.Tests.API.Tests;

[Collection(nameof(AspireFixture))]
public class SettingsTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task GetPublicSettings_ReturnsSettings()
    {
        // Arrange
        using var client = CreateAnonymousClient();

        // Act
        using var response = await client.GetAsync("/api/settings/public");

        // Assert
        var settings = await response.Content.ReadFromJsonAsync<GetPublicSettingsResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(settings);
    }

    [Fact]
    public async Task GetSettings_Anonymous_ReturnsUnauthorized()
    {
        // Arrange
        using var client = CreateAnonymousClient();

        // Act
        using var response = await client.GetAsync("/api/settings");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_UserRole_ReturnsForbidden()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;

        // Act
        using var response = await client.GetAsync("/api/settings");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_AdminRole_ReturnsSettings()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(true);
        using var client = authContext.client;

        // Act
        using var response = await client.GetAsync("/api/settings");

        // Assert
        var settings = await response.Content.ReadFromJsonAsync<GetSettingsResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(settings);
    }
}
using System.Net;
using System.Net.Http.Json;
using mPanel.API.Features.Sessions.Shared;

namespace mPanel.Tests.API.Tests;

[Collection(nameof(AspireFixture))]
public class SessionsTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task GetCurrentSession_Anonymous_ReturnsUnauthorized()
    {
        // Arrange
        using var client = CreateAnonymousClient();

        // Act
        using var response = await client.GetAsync("/api/sessions/current");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentSession_Authenticated_ReturnsSessionInfo()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;

        // Act
        using var response = await client.GetAsync("/api/sessions/current");

        // Assert
        var session = await response.Content.ReadFromJsonAsync<SessionDto>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(session);
    }
}
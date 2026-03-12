using System.Net;

namespace mPanel.Tests.API.Tests;

[Collection(nameof(AspireFixture))]
public class HealthTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task GetLive_ReturnsOk()
    {
        // Arrange
        using var client = CreateAnonymousClient();

        // Act
        using var response = await client.GetAsync("/api/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetReady_ReturnsOk()
    {
        // Arrange
        using var client = CreateAnonymousClient();

        // Act
        using var response = await client.GetAsync("/api/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
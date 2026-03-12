namespace mPanel.Tests.Web.Tests;

[Collection(nameof(AspireFixture))]
public class HomePageTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task HomePage_AnonymousUser_RedirectsToAuth()
    {
        // Act
        await Page.GotoAsync("/");

        // Assert
        await Expect(Page).ToHaveURLAsync("/auth");
    }

    [Fact]
    public async Task HomePage_AuthenticatedUser_DisplaysHomePage()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        await Page.GotoAsync("/");

        // Assert
        await Expect(Page).ToHaveURLAsync("/");
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Home");
    }
}
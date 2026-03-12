namespace mPanel.Tests.Web.Tests;

[Collection(nameof(AspireFixture))]
public class ErrorPageTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task ErrorPage_AnonymousUser_RedirectsToAuth()
    {
        // Act
        await Page.GotoAsync("/non-existent-page");

        // Assert
        await Expect(Page).ToHaveURLAsync("/auth");
    }

    [Fact]
    public async Task ErrorPage_Authenticated_ShowsPageNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        await Page.GotoAsync("/non-existent-page");

        // Assert
        await Expect(Page).ToHaveURLAsync("/non-existent-page");
        await Expect(Page.GetByText("Page not found: /non-existent-page")).ToBeVisibleAsync();
    }
}
namespace mPanel.Tests.Web.Tests;

[Collection(nameof(AspireFixture))]
public class AdminSettingsPageTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task AdminSettingsPage_AnonymousUser_RedirectsToAuth()
    {
        // Act
        await Page.GotoAsync("/admin");

        // Assert
        await Expect(Page).ToHaveURLAsync("/auth");
    }

    [Fact]
    public async Task AdminSettingsPage_UserRole_ShowsError()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        await Page.GotoAsync("/admin");

        // Assert
        await Expect(Page.GetByText("You are not supposed to be here")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AdminSettingsPage_AdminRole_ShowsSettingsForm()
    {
        // Arrange
        await AuthenticateAsync(true);

        // Act
        await Page.GotoAsync("/admin");

        // Assert
        await Expect(Page.Locator("input[name='name']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='url']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='allowRegistration']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='allowAccountSelfDeletion']")).ToBeVisibleAsync();

        await Expect(Page.Locator("input[name='smtp.host']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='smtp.port']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='smtp.username']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='smtp.password']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='smtp.from']")).ToBeVisibleAsync();
    }
}
using Microsoft.Playwright;

namespace mPanel.Tests.Web.Tests;

[Collection(nameof(AspireFixture))]
public class SettingsPageTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task SettingsPage_AnonymousUser_RedirectsToAuth()
    {
        // Act
        await Page.GotoAsync("/settings");

        // Assert
        await Expect(Page).ToHaveURLAsync("/auth");
    }

    [Fact]
    public async Task SettingsPage_AuthenticatedUser_DisplaysSettingsPage()
    {
        // Arrange
        var authContext = await AuthenticateAsync();

        // Act
        await Page.GotoAsync("/settings");

        // Assert
        var emailField = Page.Locator("input[name='email']");
        var usernameField = Page.Locator("input[name='username']");

        await Expect(Page).ToHaveURLAsync("/settings");
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Settings");
        await Expect(emailField).ToHaveValueAsync(authContext.email);
        await Expect(usernameField).ToHaveValueAsync(authContext.username);
    }

    [Fact]
    public async Task SettingsPage_UpdateProfile_WithEmptyEmailField_ShowsValidationErrorOnBlur()
    {
        // Arrange
        await AuthenticateAsync();
        await Page.GotoAsync("/settings");

        // Act
        var emailField = Page.Locator("input[name='email']");
        await emailField.FillAsync("");
        await emailField.BlurAsync();

        // Assert
        var errorMessage = Page.GetByText("Email is required", new PageGetByTextOptions { Exact = true });
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SettingsPage_UpdateProfile_WithNewUsername_UpdatesUsernameSuccessfully()
    {
        // Arrange
        var newUsername = Faker.Internet.UserName();
        await AuthenticateAsync(true);
        await Page.GotoAsync("/settings");

        // Act
        var usernameField = Page.Locator("input[name='username']");
        await usernameField.FillAsync(newUsername);
        await Page.ClickAsync("button[type='submit']");

        // Assert
        await Page.ReloadAsync();
        await Expect(usernameField).ToHaveValueAsync(newUsername);
    }

    [Fact]
    public async Task SettingsPage_UpdateProfile_WithNewEmail_ShowsModal()
    {
        // Arrange
        var newEmail = Faker.Internet.Email();
        await AuthenticateAsync(true);
        await Page.GotoAsync("/settings");

        // Act
        var emailField = Page.Locator("input[name='email']");
        await emailField.FillAsync(newEmail);
        await Page.ClickAsync("button[type='submit']");

        // Assert
        var message = Page.GetByText($"A confirmation link has been sent to {newEmail}. Click the link in that email to complete the update.");
        await Expect(message).ToBeVisibleAsync();
    }
}
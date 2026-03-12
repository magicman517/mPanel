using Microsoft.Playwright;

namespace mPanel.Tests.Web.Tests;

[Collection(nameof(AspireFixture))]
public class SettingsPasswordPageTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task SettingsPasswordPage_AnonymousUser_RedirectsToAuth()
    {
        // Act
        await Page.GotoAsync("/settings/password");
        
        // Assert
        await Expect(Page).ToHaveURLAsync("/auth");
    }
    
    [Fact]
    public async Task SettingsPasswordPage_AuthenticatedUser_DisplaysPasswordSettings()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        await Page.GotoAsync("/settings/password");
        
        // Assert
        await Expect(Page).ToHaveURLAsync("/settings/password");
        
        var currentPasswordField = Page.GetByLabel("Current Password", new PageGetByLabelOptions { Exact = true });
        var newPasswordField = Page.GetByLabel("New Password", new PageGetByLabelOptions { Exact = true });
        var confirmNewPasswordField = Page.GetByLabel("Confirm New Password", new PageGetByLabelOptions { Exact = true });
        
        await Expect(currentPasswordField).ToBeVisibleAsync();
        await Expect(newPasswordField).ToBeVisibleAsync();
        await Expect(confirmNewPasswordField).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SettingsPasswordPage_SubmitWithEmptyFields_ShowsValidationErrors()
    {
        // Arrange
        await AuthenticateAsync();
        await Page.GotoAsync("/settings/password");

        // Act
        var submitButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" });
        await submitButton.ClickAsync();

        // Assert
        var currentPasswordError =
            Page.GetByText("Current password is required", new PageGetByTextOptions { Exact = true });
        var newPasswordError = Page.GetByText("New password is required", new PageGetByTextOptions { Exact = true });
        var confirmNewPasswordError = Page.GetByText("Field cannot be empty", new PageGetByTextOptions { Exact = true });
        
        await Expect(currentPasswordError).ToBeVisibleAsync();
        await Expect(newPasswordError).ToBeVisibleAsync();
        await Expect(confirmNewPasswordError).ToBeVisibleAsync();
    }
}
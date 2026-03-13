using Microsoft.Playwright;

namespace mPanel.Tests.Web.Tests;

[Collection(nameof(AspireFixture))]
public class SettingsApiKeysPageTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task SettingsApiKeysPage_AnonymousUser_RedirectsToAuth()
    {
        // Act
        await Page.GotoAsync("/settings/api-keys");

        // Assert
        await Expect(Page).ToHaveURLAsync("/auth");
    }

    [Fact]
    public async Task SettingsApiKeysPage_AuthenticatedUser_DisplaysApiKeysPage()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        await Page.GotoAsync("/settings/api-keys");

        // Assert
        await Expect(Page).ToHaveURLAsync("/settings/api-keys");
        await Expect(Page.GetByText("No API keys found", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SettingsApiKeysPage_CreateKey_ShowsWarningModal()
    {
        // Arrange
        await AuthenticateAsync();
        await Page.GotoAsync("/settings/api-keys");

        // Act
        var createKeyButton = Page.GetByText("Create key");
        await createKeyButton.ClickAsync();
        await Expect(Page.GetByText("Create API Key", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        await Page.ClickAsync("button[type='submit']");

        // Assert
        await Expect(Page.GetByText("Copy your key now", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText(
            "This is the only time your API key will be visible. It cannot be recovered once you close this dialog.",
            new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
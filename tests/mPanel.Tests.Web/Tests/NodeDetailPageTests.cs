using Microsoft.Playwright;

namespace mPanel.Tests.Web.Tests;

[Collection(nameof(AspireFixture))]
public class NodeDetailPageTests(AspireFixture fixture) : TestBase(fixture)
{
    private async Task<string> CreateNodeViaApiAsync()
    {
        await Page.APIRequest.PutAsync("/api/settings", new APIRequestContextOptions
        {
            DataObject = new
            {
                Name = "mPanel",
                Url = "http://localhost:8080",
                AllowRegistration = true,
                AllowAccountSelfDeletion = true,
                Smtp = new { }
            }
        });

        var nodeName = $"node-{Guid.NewGuid():N}"[..20];
        var response = await Page.APIRequest.PostAsync("/api/nodes", new APIRequestContextOptions
        {
            DataObject = new
            {
                Name = nodeName,
                Scheme = "Http",
                Address = "10.0.0.1",
                Port = 10001,
                SftpPort = 2022,
                IsActive = true
            }
        });

        var json = await response.JsonAsync();
        return json?.GetProperty("id").GetString() ?? throw new Exception("Failed to create node");
    }

    [Fact]
    public async Task NodeDetailPage_AnonymousUser_RedirectsToAuth()
    {
        // Act
        await Page.GotoAsync($"/admin/nodes/{Guid.NewGuid()}");

        // Assert
        await Expect(Page).ToHaveURLAsync("/auth");
    }

    [Fact]
    public async Task NodeDetailPage_AdminRole_ShowsNodeForm()
    {
        // Arrange
        await AuthenticateAsync(true);
        var nodeId = await CreateNodeViaApiAsync();

        // Act
        await Page.GotoAsync($"/admin/nodes/{nodeId}");

        // Assert
        await Expect(Page.GetByText("Node Settings", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Node Name")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Address")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Port", new PageGetByLabelOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("SFTP Port")).ToBeVisibleAsync();

        var saveButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" });
        await Expect(saveButton).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NodeDetailPage_UpdateNode_WithShortName_ShowsValidationError()
    {
        // Arrange
        await AuthenticateAsync(true);
        var nodeId = await CreateNodeViaApiAsync();
        await Page.GotoAsync($"/admin/nodes/{nodeId}");

        // Act
        var nameField = Page.GetByLabel("Node Name");
        await nameField.FillAsync("ab");

        var saveButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" });
        await saveButton.ClickAsync();

        // Assert
        var errorMessage = Page.GetByText("Name is too short", new PageGetByTextOptions { Exact = true });
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NodeDetailPage_UpdateNode_WithValidData_ShowsSuccessToast()
    {
        // Arrange
        await AuthenticateAsync(true);
        var nodeId = await CreateNodeViaApiAsync();
        await Page.GotoAsync($"/admin/nodes/{nodeId}");

        // Act
        var nameField = Page.GetByLabel("Node Name");
        await nameField.FillAsync($"updated-{Guid.NewGuid():N}"[..20]);

        var saveButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" });
        await saveButton.ClickAsync();

        // Assert
        await Expect(Page.GetByText("Node settings have been updated", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}

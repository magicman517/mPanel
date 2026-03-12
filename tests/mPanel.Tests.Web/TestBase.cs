using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Bogus;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace mPanel.Tests.Web;

// `PWDEBUG=1 dotnet test` to run with Playwright Inspector
// `HEADED=1 dotnet test` to run with browser visible
public abstract class TestBase(AspireFixture fixture) : PageTest
{
    protected readonly DistributedApplication App = fixture.App;
    protected readonly Faker Faker = new();

    private static readonly string ResultsDir = Path.Combine(Directory.GetCurrentDirectory(), "test-results");
    private const int Timeout = 30_000;

    protected async Task<(string email, string username, string password)> AuthenticateAsync(bool asAdmin = false)
    {
        var email = Faker.Internet.Email();
        var username = Faker.Internet.UserName();
        var password = Faker.Internet.Password(length: 8);

        var path = asAdmin ? "/api/testing/create-admin-user" : "/api/users";

        await Page.APIRequest.PostAsync(path, new APIRequestContextOptions
        {
            DataObject = new
            {
                email,
                username,
                password
            }
        });

        await Page.APIRequest.PostAsync("/api/auth/sign-in", new APIRequestContextOptions
        {
            DataObject = new
            {
                identity = email,
                password
            }
        });

        return (email, username, password);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        Assertions.SetDefaultExpectTimeout(Timeout);
        Page.SetDefaultTimeout(Timeout);
        Page.SetDefaultNavigationTimeout(Timeout);
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = App.GetEndpoint("gateway", "http").ToString().TrimEnd('/'),
            ColorScheme = ColorScheme.Dark,
            RecordVideoDir = Path.Combine(ResultsDir, "videos"),
            RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 }
        };
    }

    public override async Task DisposeAsync()
    {
        var testName = $"{GetType().Name}-{Guid.NewGuid():N}";
        await Context.Tracing.StopAsync(new TracingStopOptions
        {
            Path = Path.Combine(ResultsDir, "traces", $"{testName}.zip")
        });
        await base.DisposeAsync();
    }
}
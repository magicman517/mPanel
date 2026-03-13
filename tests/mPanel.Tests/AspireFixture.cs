using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace mPanel.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class AspireFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;

    private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(1);

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.mPanel_AppHost>(["UseVolumes=false", "--environment=Testing"]);

        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddFilter("mPanel", LogLevel.Warning);
        });

        App = await builder.BuildAsync().WaitAsync(_defaultTimeout);

        await App.StartAsync().WaitAsync(_defaultTimeout);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("gateway").WaitAsync(_defaultTimeout);
    }

    public async Task DisposeAsync()
    {
        await App.DisposeAsync();
    }
}
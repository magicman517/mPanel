using System.Text.Json.Serialization;
using FastEndpoints;
using mPanel.API.Core.Interfaces;
using mPanel.API.Extensions;
using mPanel.API.Infrastructure.Jobs;
using mPanel.API.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();
builder.AddServiceHealthChecks();
builder.AddOpenApi();
builder.AddAuthentication();
if (!builder.Environment.IsEnvironment("Testing"))
    builder.AddRateLimiting();

builder.Services.AddFastEndpoints();
builder.Services.AddResponseCaching();
builder.Services.AddJobQueues<JobRecord, JobStorageProvider>();

builder.Services.AddHttpClient<INodeApiService, NodeApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsEnvironment("Testing"))
    app.UseRateLimiter();
app.UseDefaultExceptionHandler();
app.UseResponseCaching();
app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "api";
    config.Binding.UsePropertyNamingPolicy = true;
    config.Serializer.Options.Converters.Add(new JsonStringEnumConverter());

    config.Endpoints.Configurator = ep =>
    {
        if (!app.Environment.IsEnvironment("Testing"))
            ep.Options(x => x.RequireRateLimiting("Global"));
    };

    config.Endpoints.Filter = ep =>
        ep.EndpointTags?.Contains("Testing") is not true || app.Environment.IsEnvironment("Testing");

    config.Errors.UseProblemDetails();
});
app.UseJobQueues(options => { options.ExecutionTimeLimit = TimeSpan.FromMinutes(5); });
app.UseOpenApi();

app.MapFallbackToFile("index.html");

await app.MigrateDatabaseAsync();
await app.RunAsync();
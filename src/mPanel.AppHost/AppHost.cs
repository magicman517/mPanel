#pragma warning disable ASPIRECERTIFICATES001

using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var useVolumes = builder.Configuration.GetValue("UseVolumes", true);

var postgres = builder
    .AddPostgres("postgres")
    .WithHostPort(5432)
    .WithPgWeb();
if (useVolumes)
{
    postgres
        .WithDataVolume("mpanel-postgres-data");
}

var database = postgres.AddDatabase("mPanel");

var redis = builder
    .AddRedis("redis")
    .WithImageTag("8-alpine")
    .WithoutHttpsCertificate()
    .WithRedisInsight();
if (useVolumes)
{
    redis
        .WithDataVolume("mpanel-redis-data")
        .WithPersistence(interval: TimeSpan.FromMinutes(5), keysChangedThreshold: 10);
}

var mailPit = builder.AddMailPit("mailpit");
if (useVolumes)
{
    mailPit.WithDataVolume("mpanel-mailpit-data");
}

var api = builder
    .AddProject<Projects.mPanel_API>("api")
    .WithHttpHealthCheck("/api/health/ready")
    .WithReference(database)
    .WithReference(redis)
    .WithReference(mailPit)
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(mailPit);

if (builder.Environment.EnvironmentName == "Testing")
    api.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Testing");

var web = builder
    .AddJavaScriptApp("web", "../mPanel.Web")
    .WithPnpm()
    .WithHttpEndpoint(port: 5002, env: "PORT")
    .WaitFor(api);

builder
    .AddYarp("gateway")
    .WithHostPort(8080)
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/{**catch-all}", api);
        yarp.AddRoute(web);
    })
    .WaitFor(api)
    .WaitFor(web);

await builder.Build().RunAsync();
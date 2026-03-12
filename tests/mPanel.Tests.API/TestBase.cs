using System.Net.Http.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Bogus;
using mPanel.API.Features.Auth;
using mPanel.API.Features.Users;

namespace mPanel.Tests.API;

public abstract class TestBase(AspireFixture fixture)
{
    protected readonly DistributedApplication App = fixture.App;
    protected readonly Faker Faker = new();

    protected HttpClient CreateAnonymousClient()
    {
        var handler = new SocketsHttpHandler { CookieContainer = new System.Net.CookieContainer() };
        return new HttpClient(handler) { BaseAddress = App.GetEndpoint("api") };
    }

    protected async Task<(HttpClient client, string email, string username, string password)> CreateAuthenticatedClient(bool isAdmin = false)
    {
        var user = await CreateUserAsync(isAdmin: isAdmin);
        var client = await SignInAsync(user.email, user.password);
        return (client, user.email, user.username, user.password);
    }

    protected async Task<(string email, string username, string password)> CreateUserAsync(string? email = null, string? username = null, string? password = null, bool isAdmin = false)
    {
        var route = isAdmin ? "/api/testing/create-admin-user" : "/api/users";

        email ??= Faker.Internet.Email();
        username ??= Faker.Internet.UserName();
        password ??= Faker.Internet.Password(length: 8);

        using var client = CreateAnonymousClient();
        var request = new CreateUserRequest
        {
            Email = email,
            Username = username,
            Password = password
        };

        using var response = await client.PostAsJsonAsync(route, request);
        response.EnsureSuccessStatusCode();

        return (email, username, password);
    }

    private async Task<HttpClient> SignInAsync(string email, string password)
    {
        var client = App.CreateHttpClient("api");
        var signInRequest = new SignInRequest
        {
            Identity = email,
            Password = password
        };
        using var response = await client.PostAsJsonAsync("/api/auth/sign-in", signInRequest);
        response.EnsureSuccessStatusCode();

        return client;
    }
}
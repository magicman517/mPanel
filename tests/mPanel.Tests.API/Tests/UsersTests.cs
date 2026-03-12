using System.Net;
using System.Net.Http.Json;
using mPanel.API.Features.Users;

namespace mPanel.Tests.API.Tests;

[Collection(nameof(AspireFixture))]
public class UsersTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task CreateUser_WithValidData_ReturnsOk()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        var request = new CreateUserRequest
        {
            Email = Faker.Internet.Email(),
            Username = Faker.Internet.UserName(),
            Password = Faker.Internet.Password(length: 8)
        };
        
        // Act
        using var response = await client.PostAsJsonAsync("/api/users", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateUser_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        var request = new CreateUserRequest
        {
            Email = "invalid-email",
            Username = Faker.Internet.UserName(),
            Password = Faker.Internet.Password(length: 8)
        };
        
        // Act
        using var response = await client.PostAsJsonAsync("/api/users", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateUser_WithShortPassword_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        var request = new CreateUserRequest
        {
            Email = Faker.Internet.Email(),
            Username = Faker.Internet.UserName(),
            Password = Faker.Internet.Password(length: 5)
        };
        
        // Act
        using var response = await client.PostAsJsonAsync("/api/users", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateUser_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        var email = Faker.Internet.Email();
        var request1 = new CreateUserRequest
        {
            Email = email,
            Username = Faker.Internet.UserName(),
            Password = Faker.Internet.Password(length: 8)
        };
        var request2 = new CreateUserRequest
        {
            Email = email,
            Username = Faker.Internet.UserName(),
            Password = Faker.Internet.Password(length: 8)
        };
        
        // Act
        using var response1 = await client.PostAsJsonAsync("/api/users", request1);
        using var response2 = await client.PostAsJsonAsync("/api/users", request2);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }
    
    [Fact]
    public async Task CreateUser_WithExistingUsername_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        var username = Faker.Internet.UserName();
        var request1 = new CreateUserRequest
        {
            Email = Faker.Internet.Email(),
            Username = username,
            Password = Faker.Internet.Password(length: 8)
        };
        var request2 = new CreateUserRequest
        {
            Email = Faker.Internet.Email(),
            Username = username,
            Password = Faker.Internet.Password(length: 8)
        };
        
        // Act
        using var response1 = await client.PostAsJsonAsync("/api/users", request1);
        using var response2 = await client.PostAsJsonAsync("/api/users", request2);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }
    
    [Fact]
    public async Task CurrentUser_GetCurrentUserInfo_Anonymous_ReturnsUnauthorized()
    {
        // Arrange
        using var client = CreateAnonymousClient();
        
        // Act
        using var response = await client.GetAsync("/api/users/@me");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task CurrentUser_GetCurrentUserInfo_Authenticated_ReturnsUserInfo()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;
        
        // Act
        using var response = await client.GetAsync("/api/users/@me");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var userInfo = await response.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
        Assert.NotNull(userInfo);
        Assert.Equal(authContext.email, userInfo.Email);
        Assert.Equal(authContext.username, userInfo.Username);
    }
    
    [Fact]
    public async Task CurrentUser_UpdateProfile_Authenticated_ReturnsOk()
    {
        // Arrange
        var authContext = await CreateAuthenticatedClient(true); // create admin to automatically confirm email
        using var client = authContext.client;
        var request = new UpdateCurrentUserRequest
        {
            Email = Faker.Internet.Email(),
            Username = Faker.Internet.UserName()
        };
        
        // Act
        using var response = await client.PutAsJsonAsync("/api/users/@me", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task CurrentUser_UpdatePassword_Authenticated_ReturnsOk()
    {
        // Arrange
        var newPassword = Faker.Internet.Password(length: 8);
        var authContext = await CreateAuthenticatedClient();
        using var client = authContext.client;
        var request = new UpdateCurrentUserPasswordRequest
        {
            CurrentPassword = authContext.password,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        };
        
        // Act
        using var response = await client.PutAsJsonAsync("/api/users/@me/password", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
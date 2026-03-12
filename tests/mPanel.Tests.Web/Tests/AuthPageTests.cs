using Microsoft.Playwright;

namespace mPanel.Tests.Web.Tests;

[Collection(nameof(AspireFixture))]
public class AuthPageTests(AspireFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task AuthPage_AlreadyAuthenticated_RedirectsToHome()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        await Page.GotoAsync("/auth");

        // Assert
        await Expect(Page).ToHaveURLAsync("/");
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Home");
    }

    [Fact]
    public async Task AuthPage_AnonymousUser_DisplaysAuthPage()
    {
        // Act
        await Page.GotoAsync("/auth");

        // Assert
        await Expect(Page).ToHaveURLAsync("/auth");

        var title = Page.GetByText("Welcome Back!", new PageGetByTextOptions { Exact = true });
        await Expect(title).ToBeVisibleAsync();

        var description = Page.GetByText("Enter your credentials to access your account");
        await Expect(description).ToBeVisibleAsync();

        var identityField = Page.GetByLabel("Email or Username");
        await Expect(identityField).ToBeVisibleAsync();

        var passwordField = Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true });
        await Expect(passwordField).ToBeVisibleAsync();

        var submitButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue" });
        await Expect(submitButton).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AuthPage_TabNavigation_SwitchesBetweenSignInAndSignUp()
    {
        // Arrange
        await Page.GotoAsync("/auth");

        var signInTabTrigger = Page.GetByRole(AriaRole.Tab, new PageGetByRoleOptions { Name = "Sign In" });
        var signUpTabTrigger = Page.GetByRole(AriaRole.Tab, new PageGetByRoleOptions { Name = "Sign Up" });

        var signInTitle = Page.GetByText("Welcome Back!", new PageGetByTextOptions { Exact = true });
        var signUpTitle = Page.GetByText("Create an Account", new PageGetByTextOptions { Exact = true });

        await Expect(signInTitle).ToBeVisibleAsync();
        await Expect(signUpTitle).ToBeHiddenAsync();

        // Act
        await signUpTabTrigger.ClickAsync();

        // Assert
        await Expect(signInTitle).ToBeHiddenAsync();
        await Expect(signUpTitle).ToBeVisibleAsync();

        // Act
        await signInTabTrigger.ClickAsync();

        // Assert
        await Expect(signInTitle).ToBeVisibleAsync();
        await Expect(signUpTitle).ToBeHiddenAsync();
    }

    [Fact]
    public async Task AuthPage_EmptyIdentityField_ShowsValidationErrorOnBlur()
    {
        // Arrange
        await Page.GotoAsync("/auth");

        var identityField = Page.GetByLabel("Email or Username");

        // Act
        await identityField.FocusAsync();
        await identityField.BlurAsync();

        // Assert
        var errorMessage = Page.GetByText("Email or Username is required", new PageGetByTextOptions { Exact = true });
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AuthPage_ValidSignIn_SubmitsAndRedirectsToHome()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var username = Faker.Internet.UserName();
        var password = Faker.Internet.Password(length: 8);

        await Page.APIRequest.PostAsync("/api/users", new APIRequestContextOptions
        {
            DataObject = new
            {
                email,
                username,
                password
            }
        });

        await Page.GotoAsync("/auth");

        var identityField = Page.GetByLabel("Email or Username");
        var passwordField = Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true });
        var submitButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue" });

        // Act
        await identityField.FillAsync(email);
        await passwordField.FillAsync(password);
        await submitButton.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync("/");
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Home");
    }

    [Fact]
    public async Task AuthPage_ValidSignUp_SubmitsAndRedirectsToHome()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var username = Faker.Internet.UserName();
        var password = Faker.Internet.Password(length: 8);

        await Page.GotoAsync("/auth");

        var signUpTabTrigger = Page.GetByRole(AriaRole.Tab, new PageGetByRoleOptions { Name = "Sign Up" });
        await signUpTabTrigger.ClickAsync();

        var emailField = Page.GetByLabel("Email");
        var usernameField = Page.GetByLabel("Username");
        var passwordField = Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true });
        var submitButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue" });

        // Act
        await emailField.FillAsync(email);
        await usernameField.FillAsync(username);
        await passwordField.FillAsync(password);
        await submitButton.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync("/");
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Home");
    }

    [Fact]
    public async Task AuthPage_InvalidCredentials_ShowsErrorToast()
    {
        // Arrange
        await Page.GotoAsync("/auth");

        var identityField = Page.GetByLabel("Email or Username");
        var passwordField = Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true });
        var submitButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue" });

        // Act
        await identityField.FillAsync(Guid.NewGuid().ToString("N"));
        await passwordField.FillAsync("wrongpassword");
        await submitButton.ClickAsync();

        // Assert
        var errorMessage = Page.GetByText("Invalid credentials", new PageGetByTextOptions { Exact = true });
        await Expect(errorMessage).ToBeVisibleAsync();
    }
}
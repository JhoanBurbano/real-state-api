using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Million.E2E.Tests;

[TestFixture]
public class AuthenticationE2ETests : TestBase
{
    [Test]
    public async Task Owner_Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "carlos.rodriguez@million.com",
            Password = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/auth/owner/login", loginRequest);

        // Assert
        response.Should().BeSuccessful();
        response.Headers.Should().ContainKey("X-Correlation-ID");

        var loginResponse = await DeserializeResponseAsync<dynamic>(response);
        loginResponse.Should().NotBeNull();
        loginResponse!.accessToken.Should().NotBeNullOrEmpty();
        loginResponse.refreshToken.Should().NotBeNullOrEmpty();
        loginResponse.owner.Should().NotBeNull();
        loginResponse.owner!.id.Should().Be("owner-001");
        loginResponse.owner.fullName.Should().Be("Carlos Rodriguez");
        loginResponse.owner.role.Should().Be("Owner");
    }

    [Test]
    public async Task Admin_Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "admin@million.com",
            Password = "Admin123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/auth/owner/login", loginRequest);

        // Assert
        response.Should().BeSuccessful();
        
        var loginResponse = await DeserializeResponseAsync<dynamic>(response);
        loginResponse.Should().NotBeNull();
        loginResponse!.accessToken.Should().NotBeNullOrEmpty();
        loginResponse.refreshToken.Should().NotBeNullOrEmpty();
        loginResponse.owner.Should().NotBeNull();
        loginResponse.owner!.id.Should().Be("admin-001");
        loginResponse.owner.role.Should().Be("Admin");
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "invalid@email.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/auth/owner/login", loginRequest);

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.Unauthorized);
        
        var errorResponse = await DeserializeResponseAsync<dynamic>(response);
        errorResponse.Should().NotBeNull();
        errorResponse!.title.Should().Be("Invalid Credentials");
        errorResponse.status.Should().Be(401);
    }

    [Test]
    public async Task Login_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "invalid-email",
            Password = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/auth/owner/login", loginRequest);

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Refresh_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var authResponse = await AuthenticateAsOwnerAsync();
        var loginResponse = await DeserializeResponseAsync<dynamic>(authResponse);
        var refreshToken = loginResponse!.refreshToken.ToString();

        var refreshRequest = new
        {
            RefreshToken = refreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync("/auth/owner/refresh", refreshRequest);

        // Assert
        response.Should().BeSuccessful();
        
        var refreshResponse = await DeserializeResponseAsync<dynamic>(response);
        refreshResponse.Should().NotBeNull();
        refreshResponse!.accessToken.Should().NotBeNullOrEmpty();
        refreshResponse.refreshToken.Should().NotBeNullOrEmpty();
        refreshResponse.accessToken.Should().NotBe(loginResponse.accessToken.ToString());
    }

    [Test]
    public async Task Refresh_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/auth/owner/refresh", refreshRequest);

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Logout_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var authResponse = await AuthenticateAsOwnerAsync();
        var loginResponse = await DeserializeResponseAsync<dynamic>(authResponse);
        var refreshToken = loginResponse!.refreshToken.ToString();

        var logoutRequest = new
        {
            RefreshToken = refreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync("/auth/owner/logout", logoutRequest);

        // Assert
        response.Should().BeSuccessful();
    }

    [Test]
    public async Task Logout_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var logoutRequest = new
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/auth/owner/logout", logoutRequest);

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WithLockedAccount_ShouldReturnTooManyRequests()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "carlos.rodriguez@million.com",
            Password = "WrongPassword"
        };

        // Act - Try multiple failed attempts
        for (int i = 0; i < 5; i++)
        {
            var response = await Client.PostAsJsonAsync("/auth/owner/login", loginRequest);
            response.Should().HaveStatusCode(System.Net.HttpStatusCode.Unauthorized);
        }

        // Try one more time - should be locked
        var lockedResponse = await Client.PostAsJsonAsync("/auth/owner/login", loginRequest);

        // Assert
        lockedResponse.Should().HaveStatusCode(System.Net.HttpStatusCode.TooManyRequests);
        
        var errorResponse = await DeserializeResponseAsync<dynamic>(lockedResponse);
        errorResponse.Should().NotBeNull();
        errorResponse!.title.Should().Be("Too Many Requests");
    }
}


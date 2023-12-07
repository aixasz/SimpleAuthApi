using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleAuthApi.Domain.Models.Authentication;
using System.Text;
using System.Text.Json;

namespace SimpleAuthApi.Tests.IntegrationTests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Username = "thong.smith@test.com",
            Password = "P@55w0rd!"
        };
        var content = new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();

        var json = response.Content.ReadAsStringAsync().Result;
        var accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(json, jsonSerializerOptions);

        // Assert
        Assert.NotNull(accessTokenResponse);
        Assert.NotNull(accessTokenResponse.AccessToken);
    }

    [Fact]
    public async Task Login_NonExistUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Username = "Anonymous",
            Password = "F4k3P@ssw0rd!"
        };
        var content = new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsOk()
    {
        // Arrange
        var loginAccessToken = await MakeLoginAsync();
        var refreshRequest = new RefreshRequest { RefreshToken = loginAccessToken.RefreshToken };
        var content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync("/api/auth/refreshtoken", content);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(json, jsonSerializerOptions);

        // Assert
        Assert.NotNull(accessTokenResponse);
        Assert.NotNull(accessTokenResponse.AccessToken);
        Assert.NotNull(accessTokenResponse.RefreshToken);
        Assert.NotEqual(loginAccessToken.AccessToken, accessTokenResponse.AccessToken);
        Assert.NotEqual(loginAccessToken.RefreshToken, accessTokenResponse.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshRequest { RefreshToken = "InvalidRefreshToken" };
        var content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync("/api/auth/refreshtoken", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<AccessTokenResponse?> MakeLoginAsync()
    {
        var loginModel = new LoginModel
        {
            Username = "thong.smith@test.com",
            Password = "P@55w0rd!"
        };
        var loginContent = new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json");

        // Act
        var loginResponse = await httpClient.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginJson = loginResponse.Content.ReadAsStringAsync().Result;
        var accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(loginJson, jsonSerializerOptions);
        return accessTokenResponse;
    }
}

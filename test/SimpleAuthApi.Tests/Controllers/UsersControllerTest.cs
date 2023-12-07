using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleAuthApi.Domain.Models.Authentication;
using SimpleAuthApi.Domain.Models.UserManagement;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SimpleAuthApi.Tests.Controllers;

public class UsersControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public UsersControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        StringContent content = new(JsonSerializer.Serialize(new LoginModel
        {
            Username = "thong.smith@test.com",
            Password = "P@55w0rd!"
        }), Encoding.UTF8, "application/json");

        var loginResponse = httpClient.PostAsync("/api/auth/login", content).Result;

        var json = loginResponse.Content.ReadAsStringAsync().Result;
        var accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(json, jsonSerializerOptions);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResponse.AccessToken);
    }

    [Fact]
    public async Task GetById_WhenCalled_ReturnsUser()
    {
        // Arrange
        var userId = new Guid("598d4799-3f99-40a2-8bc1-f949f1ce911d");
        var expectedUrl = $"/api/users/get/{userId}";

        // Act
        var response = await httpClient.GetAsync(expectedUrl);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserViewModel>(json, jsonSerializerOptions);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
    }

    [Fact]
    public async Task GetAll_WhenCalled_ReturnsAllUsers()
    {
        // Arrange
        const string expectedUrl = "/api/users/get";

        // Act
        var response = await httpClient.GetAsync(expectedUrl);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<UserViewModel[]>(json, jsonSerializerOptions);

        // Assert
        Assert.NotNull(users);
        Assert.True(users.Length > 0);
    }

    [Fact]
    public async Task SearchWithMatched_WhenCalled_ReturnsResults()
    {
        // Arrange

        const string searchTerm = "Th";
        const string expectedUrl = "/api/users/search/";

        // Act
        var response = await httpClient.GetAsync(expectedUrl + searchTerm);

        // Assert
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<UserViewModel[]>(json, jsonSerializerOptions);

        Assert.NotNull(users);
        Assert.True(users.Length > 0);
        Assert.All(users, user => Assert.True(user.FirstName.Contains(searchTerm) || user.LastName.Contains(searchTerm)));
    }

    [Fact]
    public async Task SearchWithNotMatched_WhenCalled_ReturnsNoContent()
    {
        // Arrange
        const string searchTerm = "xyz";
        const string expectedUrl = "/api/users/search/";

        // Act
        var response = await httpClient.GetAsync(expectedUrl + searchTerm);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WhenCalled_ReturnsUpdatedUser()
    {
        // Arrange

        var userId = Guid.Parse("361940b3-d39e-440d-b1bf-70f9151fc3bf");
        var updatedUser = new UserUpdateModel
        {
            Id = userId,
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
        };
        const string expectedUrl = "/api/users/update";

        var content = new StringContent(JsonSerializer.Serialize(updatedUser), Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PutAsync(expectedUrl, content);

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var updatedUserFromResponse = JsonSerializer.Deserialize<UserViewModel>(json, jsonSerializerOptions);

        Assert.NotNull(updatedUserFromResponse);
        Assert.Equal(userId, updatedUserFromResponse.Id);
        Assert.Equal("UpdatedFirstName", updatedUserFromResponse.FirstName);
        Assert.Equal("UpdatedLastName", updatedUserFromResponse.LastName);
    }

    [Fact]
    public async Task UpdateUserWithWrongId_WhenCalled_ReturnsNotFound()
    {
        // Arrange

        var wrongUserId = Guid.NewGuid();
        var updatedUser = new UserUpdateModel
        {
            Id = wrongUserId,
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
        };
        const string expectedUrl = "/api/users/update";

        var content = new StringContent(JsonSerializer.Serialize(updatedUser), Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PutAsync(expectedUrl, content);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains($"User with id {wrongUserId} not found", responseString);
    }

    [Fact]
    public async Task DeleteExistentUser_WhenCalled_ReturnsNotFound()
    {
        // Arrange
        var nonExistentUserId = new Guid("7231653b-543f-44ed-b6d6-02c4d5e1326a");

        // Act
        var response = await httpClient.DeleteAsync("/api/users/delete/" + nonExistentUserId);
        response.EnsureSuccessStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNonExistentUser_WhenCalled_ReturnsNotFound()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid(); // Use a non-existent user ID

        // Act
        var response = await httpClient.DeleteAsync("/api/users/delete/" + nonExistentUserId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

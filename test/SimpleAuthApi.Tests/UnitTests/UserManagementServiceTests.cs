using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SimpleAuthApi.Domain;
using SimpleAuthApi.Domain.Entities;
using SimpleAuthApi.Domain.Models;
using SimpleAuthApi.Domain.Models.UserManagement;
using SimpleAuthApi.Services;

namespace SimpleAuthApi.Tests.UnitTests;

public class UserManagementServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static UserManager<User> CreateMockUserManager()
    {
        return Substitute.For<UserManager<User>>(Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        using var dbContext = CreateDbContext();
        dbContext.Users.Add(new User { Id = Guid.NewGuid(), FirstName = "Kaijeaw", LastName = "Moosub" });
        dbContext.Users.Add(new User { Id = Guid.NewGuid(), FirstName = "Kaitom", LastName = "Numpla" });
        dbContext.SaveChanges();

        var service = new UserManagementService(dbContext, CreateMockUserManager());

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Length);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoUsersExist_ShouldReturnEmptyArray()
    {
        using var dbContext = CreateDbContext();
        var service = new UserManagementService(dbContext, CreateMockUserManager());

        var result = await service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUser_WhenUserExists()
    {
        using var dbContext = CreateDbContext();
        var user = new User { Id = Guid.NewGuid(), FirstName = "Pale", LastName = "Ale" };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var service = new UserManagementService(dbContext, CreateMockUserManager());

        var result = await service.GetAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        using var dbContext = CreateDbContext();
        var service = new UserManagementService(dbContext, CreateMockUserManager());

        var result = await service.GetAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_WithMatchedName_ShouldReturnMatchingUsers()
    {
        using var dbContext = CreateDbContext();
        dbContext.Users.Add(new User { Id = Guid.NewGuid(), FirstName = "Somtum", LastName = "Kaiyang" });
        dbContext.Users.Add(new User { Id = Guid.NewGuid(), FirstName = "Larb", LastName = "Ped" });
        dbContext.SaveChanges();

        var service = new UserManagementService(dbContext, CreateMockUserManager());

        var result = await service.SearchAsync("Larb");

        Assert.Single(result);
        Assert.Contains(result, u => u.FirstName == "Larb");
    }

    [Fact]
    public async Task SearchAsync_WhenNoMatchFound_ShouldReturnEmptyArray()
    {
        using var dbContext = CreateDbContext();
        dbContext.Users.Add(new User { FirstName = "Yum", LastName = "Mooyor" });
        dbContext.SaveChanges();

        var service = new UserManagementService(dbContext, CreateMockUserManager());

        var result = await service.SearchAsync("Mango");

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("")]
    public async Task SearchAsync_WhenSearchTermIsEmpty_ShouldReturnAllUsers(string searchTerm)
    {
        using var dbContext = CreateDbContext();
        dbContext.Users.Add(new User { FirstName = "Tom", LastName = "Yum" });
        dbContext.SaveChanges();

        var service = new UserManagementService(dbContext, CreateMockUserManager());

        var result = await service.SearchAsync(searchTerm);

        Assert.Single(result);
    }

    [Fact]
    public async Task CreateAsync_WhenUserCreationPass_ShouldAddUser()
    {
        using var dbContext = CreateDbContext();
        var userManager = CreateMockUserManager();
        userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                   .Returns(Task.FromResult(IdentityResult.Success));

        var service = new UserManagementService(dbContext, userManager);
        var createModel = new UserCreateModel();

        var result = await service.CreateAsync(createModel);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_WhenUserCreationFails_ShouldThrowException()
    {
        using var dbContext = CreateDbContext();
        var userManager = CreateMockUserManager();
        userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                   .Returns(Task.FromResult(IdentityResult.Failed()));

        var service = new UserManagementService(dbContext, userManager);
        var createModel = new UserCreateModel();

        await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(createModel));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        using var dbContext = CreateDbContext();
        var user = new User { Id = Guid.NewGuid(), FirstName = "Suki", LastName = "Yaki" };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var service = new UserManagementService(dbContext, CreateMockUserManager());

        await service.DeleteAsync(user.Id);

        Assert.DoesNotContain(dbContext.Users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserNotFound_ShouldThrowException()
    {
        using var dbContext = CreateDbContext();
        var service = new UserManagementService(dbContext, CreateMockUserManager());

        await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        using var dbContext = CreateDbContext();
        var user = new User { Id = Guid.NewGuid(), FirstName = "Tumthai", LastName = "Kaikhem" };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var service = new UserManagementService(dbContext, CreateMockUserManager());
        var updateModel = new UserUpdateModel { Id = user.Id, FirstName = "Tumpoo", LastName = "Plarah" };

        var result = await service.UpdateAsync(updateModel);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ShouldThrowException()
    {
        using var dbContext = CreateDbContext();
        var service = new UserManagementService(dbContext, CreateMockUserManager());
        var updateModel = new UserUpdateModel { Id = Guid.NewGuid(), FirstName = "Pad", LastName = "Krapao" };

        await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(updateModel));
    }
}

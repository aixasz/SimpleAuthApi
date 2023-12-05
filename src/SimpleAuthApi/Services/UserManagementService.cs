using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleAuthApi.Domain;
using SimpleAuthApi.Domain.Entities;
using SimpleAuthApi.Domain.Models;

namespace SimpleAuthApi.Services;

public class UserManagementService(
    AppDbContext dbContext,
    UserManager<User> userManager,
    IUserStore<User> userStore,
    IUserEmailStore<User> emailStore) : IUserManagementService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly IUserStore<User> userStore;
    private readonly IUserEmailStore<User> emailStore;
    private readonly UserManager<User> userManager = userManager;

    public async Task<UserViewModel[]> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users.ToListAsync(cancellationToken);

        return users.Select(FromEntity).ToArray();
    }

    public async Task<UserViewModel?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
        return user is not null ? FromEntity(user) : null;
    }

    public async Task<UserViewModel[]> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users.Where(user =>
            user.FirstName.Contains(term) || user.LastName.Contains(term)
        ).ToArrayAsync(cancellationToken);

        return users.Select(FromEntity).ToArray();
    }

    public async Task<UserViewModel> CreateAsync(UserCreateModel createModel, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            FirstName = createModel.FirstName,
            LastName = createModel.LastName,
        };

        await userStore.SetUserNameAsync(user, createModel.Email, cancellationToken);
        await emailStore.SetEmailAsync(user, createModel.Email, cancellationToken);

        var result = await userManager.CreateAsync(user, createModel.Password);

        if (!result.Succeeded)
        {
            var message = string.Join(
                Environment.NewLine,
                result.Errors.Select(error => $"{error.Code} : {error.Description}")
            );
            throw new Exception(message);
        }

        return FromEntity(user);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
        if (user is not null)
        {
            dbContext.Remove(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserViewModel> UpdateAsync(UserViewModel userUpdateModel, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(user => user.Id == userUpdateModel.Id, cancellationToken);
        if (user is not null)
        {
            user.FirstName = userUpdateModel.FirstName;
            user.LastName = userUpdateModel.LastName;

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return FromEntity(user);
        }

        return userUpdateModel;
    }

    private static UserViewModel FromEntity(User user) => new()
    {
        Email = user.Email!,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Id = user.Id,
    };
}

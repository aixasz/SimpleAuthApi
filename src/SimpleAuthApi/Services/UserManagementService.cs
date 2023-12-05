using SimpleAuthApi.Database;

namespace SimpleAuthApi.Services;

public interface IUserManagementService
{
    Task<User> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> SearchAsync(string term, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}

public class UserManagementService : IUserManagementService
{ 
    private readonly AppDbContext dbContext;

    public UserManagementService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<User>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


    public Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

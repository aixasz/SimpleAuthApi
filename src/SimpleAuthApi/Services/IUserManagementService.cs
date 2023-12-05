using SimpleAuthApi.Domain.Models;

namespace SimpleAuthApi.Services;

public interface IUserManagementService
{
    Task<UserViewModel?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserViewModel[]> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserViewModel[]> SearchAsync(string term, CancellationToken cancellationToken = default);
    Task<UserViewModel> CreateAsync(UserCreateModel userCreateModel, CancellationToken cancellationToken = default);
    Task<UserViewModel> UpdateAsync(UserViewModel userUpdateModel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}

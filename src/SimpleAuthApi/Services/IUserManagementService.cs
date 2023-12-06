using SimpleAuthApi.Domain.Models.UserManagement;

namespace SimpleAuthApi.Services;

public interface IUserManagementService
{
    Task<UserViewModel?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserViewModel[]> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserViewModel[]> SearchAsync(string term, CancellationToken cancellationToken = default);
    Task<UserViewModel> CreateAsync(UserCreateModel userCreateModel, CancellationToken cancellationToken = default);
    Task<UserViewModel> UpdateAsync(UserUpdateModel userUpdateModel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}

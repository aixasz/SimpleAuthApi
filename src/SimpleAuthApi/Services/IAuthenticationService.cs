using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using SimpleAuthApi.Domain.Models.Authentication;

namespace SimpleAuthApi.Services;

public interface IAuthenticationService
{
    Task<AccessTokenResponse> LoginAsync(LoginModel loginModel, CancellationToken cancellationToken = default);
    Task<AccessTokenResponse> RefreshTokenAsync(RefreshRequest refreshRequest, CancellationToken cancellationToken = default);
}

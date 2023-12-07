using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using SimpleAuthApi.Domain.Entities;
using SimpleAuthApi.Domain;
using SimpleAuthApi.Domain.Models.Authentication;
using Microsoft.Extensions.Options;
using SimpleAuthApi.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SimpleAuthApi.Services;

public interface IAuthenticationService
{
    Task<AccessTokenResponse> LoginAsync(LoginModel loginModel, CancellationToken cancellationToken = default);
    Task<AccessTokenResponse> RefreshTokenAsync(RefreshRequest refreshRequest, CancellationToken cancellationToken = default);
}

public class AuthenticationService(
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    IOptions<JwtSettings> jwtOptions,
    AppDbContext dbContext) : IAuthenticationService
{
    private readonly UserManager<User> userManager = userManager;
    private readonly SignInManager<User> signInManager = signInManager;
    private readonly AppDbContext dbContext = dbContext;
    private readonly JwtSettings jwtOptions = jwtOptions.Value;

    public async Task<AccessTokenResponse> LoginAsync(LoginModel loginModel, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(loginModel.Username)
                ?? await userManager.FindByNameAsync(loginModel.Username);

        var result = await signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);

        if (!result.Succeeded)
        {
           throw new Exception($"Unauthorized : {result}");
        }

        // Generate the access token
        var accessToken = GenerateAccessToken(user);
        var expiresIn = jwtOptions.AccessTokenExpiresInMinutes;
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, cancellationToken);

        return new AccessTokenResponse()
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken
        };
    }

    public async Task<AccessTokenResponse> RefreshTokenAsync(RefreshRequest refreshRequest, CancellationToken cancellationToken = default)
    {
        var user = await ValidateRefreshTokenAsync(refreshRequest.RefreshToken, cancellationToken)
                ?? throw new Exception("Unauthorized : Invalid refresh token");
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, cancellationToken);

        var response = new AccessTokenResponse
        {
            AccessToken = newAccessToken,
            ExpiresIn = jwtOptions.AccessTokenExpiresInMinutes,
            RefreshToken = newRefreshToken
        };

        return response;
    }

    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtOptions.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new(JwtRegisteredClaimNames.Sub, user.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }),
            Expires = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenExpiresInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddMinutes(jwtOptions.RefreshTokenExpiresInMinutes),
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return refreshToken.Token;
    }

    private async Task<User> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await dbContext.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == token, cancellationToken);

        if (refreshToken?.IsRevoked != false || refreshToken.ExpiryDate <= DateTime.UtcNow)
        {
            return null;
        }

        return refreshToken.User;
    }
}

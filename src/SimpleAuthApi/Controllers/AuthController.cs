using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleAuthApi.Domain;
using SimpleAuthApi.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SimpleAuthApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    ILogger<AuthController> logger,
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    IConfiguration configuration,
    AppDbContext dbContext) : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly UserManager<User> userManager = userManager;
    private readonly SignInManager<User> signInManager = signInManager;
    private readonly ILogger<AuthController> logger = logger;
    private readonly AppDbContext dbContext = dbContext;

    [HttpPost]
    public async Task<ActionResult<AccessTokenResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

        var user = await userManager.FindByEmailAsync(loginRequest.Email);

        var result = await signInManager.PasswordSignInAsync(user, loginRequest.Password, false, true);

        if (!result.Succeeded)
        {
            return Unauthorized(result.ToString());
        }

        // Generate the access token
        var accessToken = GenerateAccessToken(user);
        var expiresIn = GetExpiresInMinutes();
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AccessTokenResponse()
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken
        };
    }

    [HttpPost]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
    {
        var user = await ValidateRefreshTokenAsync(request.RefreshToken);
        if (user == null)
        {
            return Unauthorized("Invalid refresh token.");
        }

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

        // Construct the response
        var response = new AccessTokenResponse
        {
            AccessToken = newAccessToken,
            ExpiresIn = GetExpiresInMinutes(), // Assuming this returns the lifetime of the access token in minutes
            RefreshToken = newRefreshToken
        };

        return Ok(response);
    }

    private long GetExpiresInMinutes()
    {
        return int.TryParse(configuration["TokenExpiresInMinutes"], out var result) ? result : 120;
    }

    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new(JwtRegisteredClaimNames.Sub, user.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }),
            Expires = DateTime.UtcNow.AddMinutes(GetExpiresInMinutes()),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        return refreshToken.Token;
    }

    private async Task<User> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await dbContext.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == token);

        if (refreshToken?.IsRevoked != false || refreshToken.ExpiryDate <= DateTime.UtcNow)
        {
            return null; // Token is not valid
        }

        return refreshToken.User;
    }



}

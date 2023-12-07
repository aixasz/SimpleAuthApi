using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthApi.Domain.Models.Authentication;
using SimpleAuthApi.Services;

namespace SimpleAuthApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    private readonly IAuthenticationService authenticationService = authenticationService;

    [HttpPost]
    public async Task<ActionResult<AccessTokenResponse>> Login([FromBody] LoginModel loginModel)
    {
        try
        {
            var accessToken = await authenticationService.LoginAsync(loginModel, HttpContext.RequestAborted);
            return Ok(accessToken);
        }
        catch (Exception ex)
        {
            if (ex.Message.StartsWith("Unauthorized"))
            {
                return Unauthorized(ex.Message);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
    {
        try
        {
            var accessToken = await authenticationService.RefreshTokenAsync(request, HttpContext.RequestAborted);
            return Ok(accessToken);
        }
        catch (Exception ex)
        {
            if (ex.Message.StartsWith("Unauthorized"))
            {
                return Unauthorized(ex.Message);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

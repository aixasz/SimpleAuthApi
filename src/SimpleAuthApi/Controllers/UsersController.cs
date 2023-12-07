using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthApi.Domain.Models.UserManagement;
using SimpleAuthApi.Services;

namespace SimpleAuthApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService userManagementService;
    private readonly ILogger<UsersController> logger;

    public UsersController(IUserManagementService userManagementService, ILogger<UsersController> logger)
    {
        this.userManagementService = userManagementService;
        this.logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserViewModel>> Get(Guid id)
    {
        return await userManagementService.GetAsync(id, HttpContext.RequestAborted);
    }

    [HttpGet]
    public async Task<ActionResult<UserViewModel[]>> Get()
    {
        var result = await userManagementService.GetAllAsync(HttpContext.RequestAborted);
        return result.Length != 0
            ? Ok(result)
            : NoContent();
    }

    [HttpGet("{term}")]
    public async Task<ActionResult<UserViewModel[]>> Search(string term)
    {
        var result = await userManagementService.SearchAsync(term, HttpContext.RequestAborted);
        return result.Length != 0
            ? Ok(result)
            : NoContent();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserViewModel>> Create([FromBody] UserCreateModel model)
    {
        var result = await userManagementService.CreateAsync(model, HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<UserViewModel>> Update([FromBody] UserUpdateModel model)
    {
        try
        {
            var result = await userManagementService.UpdateAsync(model, HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains($"User with id {model.Id} not found"))
            {
                return NotFound(ex.Message);
            }

            logger.LogError(ex, "Error updating user");
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await userManagementService.DeleteAsync(id, HttpContext.RequestAborted);
            return Ok();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains($"User with id {id} not found"))
            {
                return NotFound(ex.Message);
            }

            logger.LogError(ex, "Error updating user");
            return StatusCode(500);
        }
    }
}

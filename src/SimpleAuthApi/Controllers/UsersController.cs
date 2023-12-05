using Microsoft.AspNetCore.Mvc;
using SimpleAuthApi.Domain.Models;
using SimpleAuthApi.Services;

namespace SimpleAuthApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        this.userManagementService = userManagementService;
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
    public async Task<ActionResult<UserViewModel>> Create([FromBody] UserCreateModel model)
    {
        var result = await userManagementService.CreateAsync(model, HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<UserViewModel>> Update([FromBody] UserViewModel model)
    {
        var result = await userManagementService.UpdateAsync(model, HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] Guid id)
    {
        await userManagementService.DeleteAsync(id, HttpContext.RequestAborted);
        return Ok();
    }
}

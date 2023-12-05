using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthApi.Domain.Entities;

namespace SimpleAuthApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    ILogger<AuthController> logger,
    SignInManager<User> signInManager,
    UserManager<User> userManager) : ControllerBase
{
    private readonly UserManager<User> userManager = userManager;
    private readonly SignInManager<User> signInManager = signInManager;
    private readonly ILogger<AuthController> logger = logger;





}

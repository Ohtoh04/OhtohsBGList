using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}auth")]
public class AuthController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApiUser> _userManager;

    public AuthController(RoleManager<IdentityRole> roleManager, UserManager<ApiUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpPost]
    public IActionResult Seed(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

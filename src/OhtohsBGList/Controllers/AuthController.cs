using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OhtohsBGList.Constants;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApiUser> _userManager;

    public AuthController(RoleManager<IdentityRole> roleManager, UserManager<ApiUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        int rolesCreated = 0;
        int usersAddedToRoles = 0;

        if (!await _roleManager.RoleExistsAsync(RoleNames.Moderator))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(RoleNames.Moderator));
            rolesCreated++;
        }

        if (!await _roleManager.RoleExistsAsync(RoleNames.Administrator))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(RoleNames.Administrator));
            rolesCreated++;
        }

        var testModerator = await _userManager.FindByNameAsync("testmoderator@gmail.com");
        if (testModerator is not null &&
            !await _userManager.IsInRoleAsync(
                testModerator, RoleNames.Moderator))
        {
            await _userManager.AddToRoleAsync(testModerator, RoleNames.Moderator);
            usersAddedToRoles++;
        }

        var testAdministrator = await _userManager.FindByNameAsync("testadministrator@gmail.com");
        if (testAdministrator is not null &&
            !await _userManager.IsInRoleAsync(
                testAdministrator, RoleNames.Administrator))
        {
            await _userManager.AddToRoleAsync(testAdministrator, RoleNames.Moderator);
            await _userManager.AddToRoleAsync(testAdministrator, RoleNames.Administrator);
            usersAddedToRoles++;
        }

        return Ok(new
        {
            RolesCreated = rolesCreated,
            UsersAddedToRoles = usersAddedToRoles
        });
    }

//[HttpPatch("profile")]
    //public async 
}


/*
 testadministrator@gmail.com1234Aa
 */

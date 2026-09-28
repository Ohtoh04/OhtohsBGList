using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OhtohsBGList.Constants;
using OhtohsBGList.Contracts.Account;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/accounts")]
public class AccountController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApiUser> _userManager;

    public AccountController(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApiUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Creates the Moderator/Administrator roles if missing and assigns them to the built-in test accounts.
    /// </summary>
    [Authorize(Roles = RoleNames.Administrator)]
    [ResponseCache(CacheProfileName = "NoCache")]
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
            await _userManager.AddToRoleAsync(testAdministrator, RoleNames.Administrator);
            usersAddedToRoles++;
        }

        return Ok(new
        {
            RolesCreated = rolesCreated,
            UsersAddedToRoles = usersAddedToRoles
        });
    }

    /// <summary>
    /// Changes the current user's phone number using a verification token.
    /// </summary>
    /// <param name="request">The new phone number and its verification token.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPatch("phone-number")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePhoneNumber(
        [FromBody] UpdatePhoneNumberRequest request,
        CancellationToken ct)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var result = await _userManager.ChangePhoneNumberAsync(
            user,
            request.PhoneNumber,
            request.Token);

        if (!result.Succeeded)
            return MapIdentityErrors(result);

        return NoContent();
    }

    /// <summary>
    /// Changes the current user's username.
    /// </summary>
    /// <param name="userName">The new username.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPatch("username")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUsername(
    [FromBody] string userName,
    CancellationToken ct)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var result = await _userManager.SetUserNameAsync(user, userName);

        if (!result.Succeeded)
            return MapIdentityErrors(result);

        return NoContent();
    }

    private IActionResult MapIdentityErrors(IdentityResult result)
    {
        // Invalid/expired verification token (phone change)
        if (result.Errors.Any(e => e.Code is "InvalidToken" or "ExpiredToken"))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid or expired verification token.",
                extensions: new Dictionary<string, object?> { ["code"] = "invalid_token" });
        }

        // Duplicate identifiers
        var duplicateError = result.Errors.FirstOrDefault(e =>
            e.Code is "DuplicateUserName"
                    or "DuplicateEmail"
                    or "DuplicatePhoneNumber"
                    or "PhoneNumberAlreadyInUse");

        if (duplicateError is not null)
        {
            var (title, code) = duplicateError.Code switch
            {
                "DuplicateUserName" => ("This username is already taken.", "username_in_use"),
                "DuplicateEmail" => ("This email is already in use.", "email_in_use"),
                _ => ("This phone number is already in use.", "phone_in_use"),
            };

            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: title,
                extensions: new Dictionary<string, object?> { ["code"] = code });
        }

        return ValidationProblem(new ValidationProblemDetails(
            result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description })));
    }
}
/*
 testadministrator@gmail.com1234Aa
 */

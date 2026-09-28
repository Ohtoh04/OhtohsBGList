using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Services;

public class ApiUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApiUser, IdentityRole>
{
    public ApiUserClaimsPrincipalFactory(
        UserManager<ApiUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) { }

    public override async Task<ClaimsPrincipal> CreateAsync(ApiUser user)
    {
        var principal = await base.CreateAsync(user);
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            ((ClaimsIdentity)principal.Identity!).AddClaim(
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
        }
        return principal;
    }
}

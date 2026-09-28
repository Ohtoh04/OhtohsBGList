using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Authorization;

namespace OhtohsBGList.Endpoints;

public static class DebugEndpoints
{
    private static readonly Dictionary<string, string> WellKnownClaimTypeNames =
        typeof(ClaimTypes)
            .GetFields()
            .ToDictionary(f => (string)f.GetValue(null)!, f => f.Name);

    public static WebApplication AddDebugEndpoints(this WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        app.MapGet("api/v{apiVersion:apiVersion}/auth/identity-info", GetIdentityInfo)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1));

        return app;
    }

    private static async Task<IResult> GetIdentityInfo(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        string? policy)
    {
        var user = httpContext.User;

        var claims = user.Claims
            .Select(c => new
            {
                Type = WellKnownClaimTypeNames.GetValueOrDefault(c.Type, c.Type),
                c.Value,
            })
            .OrderBy(c => c.Type)
            .ToList();

        string? policyResult = null;
        if (!string.IsNullOrWhiteSpace(policy))
        {
            try
            {
                var result = await authorizationService.AuthorizeAsync(user, policy);
                policyResult = result.Succeeded ? "Succeeded" : "Failed";
            }
            catch (InvalidOperationException)
            {
                return Results.BadRequest($"Unknown authorization policy '{policy}'.");
            }
        }

        return Results.Ok(new
        {
            IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
            AuthenticationType = user.Identity?.AuthenticationType,
            Name = user.Identity?.Name,
            Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value),
            Claims = claims,
            PolicyChecked = policy,
            PolicyResult = policyResult,
        });
    }
}

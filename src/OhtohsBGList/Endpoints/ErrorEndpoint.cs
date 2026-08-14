using Microsoft.AspNetCore.Cors;

namespace OhtohsBGList.Endpoints;

public static class ErrorEndpoint
{
    public static void AddErrorEndpoint(this WebApplication app)
    {
        app.MapGet("/error", [EnableCors("GetOnly")] () => Results.Problem());
    }
}

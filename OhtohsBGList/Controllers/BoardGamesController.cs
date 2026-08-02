using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/board-games")]
public class BoardGamesController(ILogger<BoardGamesController> logger)
    : ControllerBase
{
    ILogger<BoardGamesController> _logger = logger;


    [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 64900)]
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<BoardGame> Get()
    {
        _logger.LogInformation("Fetching board gays");
        return [];
    }
}

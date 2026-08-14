using System.Globalization;
using Asp.Versioning;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OhtohsBGList.Contracts;
using OhtohsBGList.Contracts.Csv;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Models;
using System.Linq.Dynamic.Core;
using OhtohsBGList.Contracts.BoardGames;
using OhtohsBGList.Mappings;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/board-games")]
public class BoardGamesController(
    ILogger<BoardGamesController> logger,
    BgDbContext dbContext)
    : ControllerBase
{
    private readonly ILogger<BoardGamesController> _logger = logger;
    private readonly BgDbContext _context = dbContext;

    [HttpPost("bulk", Name = "UploadBoardGames")]
    public async Task<IActionResult> UploadBoardGames(IFormFile data, CancellationToken ct)
    {
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("pt-BR"))
        {
            HasHeaderRecord = true,
            Delimiter = ";",
        };

        using var stream = data.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        var existingBoardGames = await _context.BoardGames
            .ToDictionaryAsync(bg => bg.Id, ct);
        var existingDomains = await _context.Domains
            .ToDictionaryAsync(d => d.Name, ct);
        var existingMechanics = await _context.Mechanics
            .ToDictionaryAsync(m => m.Name, ct);

        var skippedRows = 0;

        await foreach (var record in csv.GetRecordsAsync<BggRecord>(ct))
        {
            if (!record.ID.HasValue
                || string.IsNullOrEmpty(record.Name)
                || existingBoardGames.ContainsKey(record.ID.Value))
            {
                skippedRows++;
                continue;
            }

            var boardGame = new BoardGame()
            {
                Id = record.ID.Value,
                Name = record.Name,
                BGGRank = record.BGGRank ?? 0,
                ComplexityAverage = record.ComplexityAverage ?? 0,
                MaxPlayers = record.MaxPlayers ?? 0,
                MinAge = record.MinAge ?? 0,
                MinPlayers = record.MinPlayers ?? 0,
                OwnedUsers = record.OwnedUsers ?? 0,
                PlayTime = record.PlayTime ?? 0,
                RatingAverage = record.RatingAverage ?? 0,
                UsersRated = record.UsersRated ?? 0,
                Year = record.YearPublished ?? 0,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
            };
            _context.BoardGames.Add(boardGame);

            if (!string.IsNullOrEmpty(record.Domains))
                foreach (var domainName in record.Domains
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.InvariantCultureIgnoreCase))
                {
                    var domain = existingDomains.GetValueOrDefault(domainName);
                    if (domain == null)
                    {
                        domain = new Domain()
                        {
                            Name = domainName,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow
                        };
                        _context.Domains.Add(domain);
                        existingDomains.Add(domainName, domain);
                    }
                    _context.BoardGames_Domains.Add(new BoardGames_Domains()
                    {
                        BoardGame = boardGame,
                        Domain = domain,
                        CreatedDate = DateTime.UtcNow
                    });
                }

            if (!string.IsNullOrEmpty(record.Mechanics))
                foreach (var mechanicName in record.Mechanics
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.InvariantCultureIgnoreCase))
                {
                    var mechanic = existingMechanics.GetValueOrDefault(mechanicName);
                    if (mechanic == null)
                    {
                        mechanic = new Mechanic()
                        {
                            Name = mechanicName,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow
                        };
                        _context.Mechanics.Add(mechanic);
                        existingMechanics.Add(mechanicName, mechanic);
                    }
                    _context.BoardGames_Mechanics.Add(new BoardGames_Mechanics()
                    {
                        BoardGame = boardGame,
                        Mechanic = mechanic,
                        CreatedDate = DateTime.UtcNow
                    });
                }
        }

        using var transaction = _context.Database.BeginTransaction();
        await _context.SaveChangesAsync(ct);
        transaction.Commit();

        // RECAP
        return new JsonResult(new
        {
            BoardGames = _context.BoardGames.Count(),
            Domains = _context.Domains.Count(),
            Mechanics = _context.Mechanics.Count(),
            SkippedRows = skippedRows
        });
    }


    [HttpGet(Name = "GetBoardGames")]
    public async Task<ActionResult<ApiResponse<PagedResponse<BoardGame>>>> GetBoardGames(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortColumn = "Name",
        CancellationToken ct = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _context.BoardGames.CountAsync(ct);
        var result = await _context.BoardGames
            .OrderBy(sortColumn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var pagedResponse = new PagedResponse<BoardGame>(
            result,
            pageNumber,
            pageSize,
            totalCount);

        var links = new List<Link>
        {
            new Link
            {
                Href = Url.Link("GetBoardGames", new { pageNumber, pageSize, sortColumn })!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            }
        };

        if (pagedResponse.HasNextPage)
            links.Add(new Link
            {
                Href = Url.Link("GetBoardGames", new { pageNumber = pageNumber + 1, pageSize, sortColumn })!,
                Method = HttpMethod.Get.Method,
                Rel = "next"
            });

        if (pagedResponse.HasPreviousPage)
            links.Add(new Link
            {
                Href = Url.Link("GetBoardGames", new { pageNumber = pageNumber - 1, pageSize, sortColumn })!,
                Method = HttpMethod.Get.Method,
                Rel = "prev"
            });

        var response = new ApiResponse<PagedResponse<BoardGame>>()
        {
            Data = pagedResponse,
            Links = links
        };

        return Ok(response);
    }

    [HttpPost(Name = "CreateBoardGame")]
    public async Task<IActionResult> CreateBoardGame(
        [FromBody] CreateBoardGameRequest request,
        CancellationToken ct = default)
    {
        var boardGame = request.ToBoardGame();

        await _context.BoardGames.AddAsync(boardGame, ct);
        await _context.SaveChangesAsync(ct);

        var response = new ApiResponse<CreateBoardGameResponse>()
        {
            Data = boardGame.ToCreateBoardGameResponse(),
            Links = GetBoardGameLinks(Url, boardGame.Id)
        };

        return CreatedAtAction(nameof(GetBoardGameById), new { id = boardGame.Id }, response);
    }

    [HttpGet("{id:int}", Name = "GetBoardGameById")]
    public async Task<IActionResult> GetBoardGameById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var boardGame = await _context.BoardGames.FindAsync([id], ct);

        if (boardGame is null)
            return NotFound();

        var response = new ApiResponse<BoardGame>()
        {
            Data = boardGame,
            Links = GetBoardGameLinks(Url, id)
        };

        return Ok(response);
    }

    [HttpPut("{id:int}", Name = "UpdateBoardGame")]
    public async Task<IActionResult> UpdateBoardGame(
        [FromRoute] int id,
        [FromBody] UpdateBoardGameRequest request,
        CancellationToken ct = default)
    {
        var boardGame = await _context.BoardGames.FindAsync([id], ct);

        if (boardGame is null)
            return NotFound();

        boardGame.ApplyUpdate(request);
        await _context.SaveChangesAsync(ct);

        var response = new ApiResponse<UpdateBoardGameResponse>()
        {
            Data = boardGame.ToUpdateBoardGameResponse(),
            Links = GetBoardGameLinks(Url, id)
        };

        return Ok(response);
    }

    [HttpDelete("{id:int}", Name = "DeleteBoardGameById")]
    public async Task<IActionResult> DeleteBoardGameById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var boardGame = await _context.BoardGames.FindAsync([id], ct);

        if (boardGame is null)
            return NotFound();

        _context.BoardGames.Remove(boardGame);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    private static List<Link> GetBoardGameLinks(IUrlHelper url, int id)
    {
        return new List<Link>
        {
            new Link
            {
                Href = url.Link("GetBoardGameById", new { id })!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            },
            new Link
            {
                Href = url.Link("UpdateBoardGame", new { id })!,
                Method = HttpMethod.Put.Method,
                Rel = "update"
            },
            new Link
            {
                Href = url.Link("DeleteBoardGameById", new { id })!,
                Method = HttpMethod.Delete.Method,
                Rel = "delete"
            }
        };
    }
}

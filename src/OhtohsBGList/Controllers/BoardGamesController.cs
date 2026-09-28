using System.Globalization;
using Asp.Versioning;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OhtohsBGList.Constants;
using OhtohsBGList.Contracts;
using OhtohsBGList.Contracts.Csv;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Models;
using System.Linq.Dynamic.Core;
using OhtohsBGList.Contracts.BoardGames;
using OhtohsBGList.Extensions;
using OhtohsBGList.Mappings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Authorization;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/board-games")]
public class BoardGamesController(
    ILogger<BoardGamesController> logger,
    BgDbContext dbContext,
    IDistributedCache cache)
    : ControllerBase
{
    private readonly ILogger<BoardGamesController> _logger = logger;
    private readonly BgDbContext _context = dbContext;
    private readonly IDistributedCache _cache = cache;

    /// <summary>
    /// Bulk-imports board games (plus their domains and mechanics) from a BGG-format CSV file.
    /// </summary>
    /// <param name="data">The CSV file to import.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Administrator)]
    [ResponseCache(CacheProfileName = "NoCache")]
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

    /// <summary>
    /// Gets a paged, sorted and optionally filtered list of board games.
    /// </summary>
    /// <param name="request">Paging, sorting and filtering parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Policy = "ModeratorWithMobilePhone")]
    [ResponseCache(CacheProfileName = "Any-60")]
    [HttpGet(Name = "GetBoardGames")]
    public async Task<ActionResult<ApiResponse<PagedResponse<BoardGame>>>> GetBoardGames(
        [FromQuery] PagedRequest<BoardGame> request,
        CancellationToken ct = default)
    {
        var cacheKey =
            $"{nameof(GetBoardGames)}-{request.PageNumber}-{request.PageSize}-{request.SortColumn}-{request.SortOrder}-{request.FilterQuery}";

        if (!_cache.TryGetValue(cacheKey, out PagedResponse<BoardGame>? pagedResponse))
        {
            var query = _context.BoardGames.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.FilterQuery))
                query = query.Where(bg => bg.Name.Contains(request.FilterQuery));

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy($"{request.SortColumn} {request.SortOrder}")
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            pagedResponse = new PagedResponse<BoardGame>(
                items,
                request.PageNumber,
                request.PageSize,
                totalCount);

            _cache.Set(cacheKey, pagedResponse, TimeSpan.FromSeconds(60));
        }

        object RouteValuesForPage(int pageNumber) => new
        {
            pageNumber,
            request.PageSize,
            request.SortColumn,
            request.SortOrder,
            request.FilterQuery
        };

        var links = new List<Link>
        {
            new Link
            {
                Href = Url.Link("GetBoardGames", RouteValuesForPage(request.PageNumber))!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            }
        };

        if (pagedResponse.HasNextPage)
            links.Add(new Link
            {
                Href = Url.Link("GetBoardGames", RouteValuesForPage(request.PageNumber + 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "next"
            });

        if (pagedResponse.HasPreviousPage)
            links.Add(new Link
            {
                Href = Url.Link("GetBoardGames", RouteValuesForPage(request.PageNumber - 1))!,
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

    /// <summary>
    /// Creates a new board game.
    /// </summary>
    /// <param name="request">The board game's name and year.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
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

    /// <summary>
    /// Gets a single board game by id.
    /// </summary>
    /// <param name="id">The board game's id.</param>
    /// <param name="ct">A cancellation token.</param>
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

    /// <summary>
    /// Updates an existing board game's name and/or year.
    /// </summary>
    /// <param name="id">The board game's id.</param>
    /// <param name="request">The fields to update.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
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

    /// <summary>
    /// Deletes a board game by id.
    /// </summary>
    /// <param name="id">The board game's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Administrator)]
    [ResponseCache(CacheProfileName = "NoCache")]
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

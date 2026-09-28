using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;
using OhtohsBGList.Constants;
using OhtohsBGList.Contracts;
using OhtohsBGList.Contracts.Mechanics;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Models;
using OhtohsBGList.Extensions;
using OhtohsBGList.Mappings;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mechanics")]
public class MechanicsController(
    ILogger<MechanicsController> logger,
    BgDbContext dbContext,
    IDistributedCache cache)
    : ControllerBase
{
    private readonly ILogger<MechanicsController> _logger = logger;
    private readonly BgDbContext _context = dbContext;
    private readonly IDistributedCache _cache = cache;

    /// <summary>
    /// Gets a paged, sorted and optionally filtered list of mechanics.
    /// </summary>
    /// <param name="request">Paging, sorting and filtering parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    [ResponseCache(CacheProfileName = "Any-60")]
    [HttpGet(Name = "GetMechanics")]
    public async Task<IActionResult> GetMechanics(
        [FromQuery] PagedRequest<Mechanic> request,
        CancellationToken ct = default)
    {
        var cacheKey =
            $"{nameof(GetMechanics)}-{request.PageNumber}-{request.PageSize}-{request.SortColumn}-{request.SortOrder}-{request.FilterQuery}";

        if (!_cache.TryGetValue(cacheKey, out PagedResponse<Mechanic>? pagedResponse))
        {
            var query = _context.Mechanics.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.FilterQuery))
                query = query.Where(m => m.Name.Contains(request.FilterQuery));

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy($"{request.SortColumn} {request.SortOrder}")
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            pagedResponse = new PagedResponse<Mechanic>(items, request.PageNumber, request.PageSize, totalCount);

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
                Href = Url.Link("GetMechanics", RouteValuesForPage(request.PageNumber))!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            }
        };

        if (pagedResponse.HasNextPage)
            links.Add(new Link
            {
                Href = Url.Link("GetMechanics", RouteValuesForPage(request.PageNumber + 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "next"
            });

        if (pagedResponse.HasPreviousPage)
            links.Add(new Link
            {
                Href = Url.Link("GetMechanics", RouteValuesForPage(request.PageNumber - 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "prev"
            });

        return Ok(new ApiResponse<PagedResponse<Mechanic>> { Data = pagedResponse, Links = links });
    }

    /// <summary>
    /// Gets a single mechanic by id.
    /// </summary>
    /// <param name="id">The mechanic's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [HttpGet("{id:int}", Name = "GetMechanicById")]
    public async Task<IActionResult> GetMechanicById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var mechanic = await _context.Mechanics.FindAsync([id], ct);

        if (mechanic is null)
            return NotFound();

        return Ok(new ApiResponse<Mechanic> { Data = mechanic, Links = GetMechanicLinks(Url, id) });
    }

    /// <summary>
    /// Creates a new mechanic.
    /// </summary>
    /// <param name="request">The mechanic's name, notes and flags.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPost(Name = "CreateMechanic")]
    public async Task<IActionResult> CreateMechanic(
        [FromBody] CreateMechanicRequest request,
        CancellationToken ct = default)
    {
        var mechanic = request.ToMechanic();

        await _context.Mechanics.AddAsync(mechanic, ct);
        await _context.SaveChangesAsync(ct);

        var response = new ApiResponse<CreateMechanicResponse>
        {
            Data = mechanic.ToCreateMechanicResponse(),
            Links = GetMechanicLinks(Url, mechanic.Id)
        };

        return CreatedAtAction(nameof(GetMechanicById), new { id = mechanic.Id }, response);
    }

    /// <summary>
    /// Updates an existing mechanic.
    /// </summary>
    /// <param name="id">The mechanic's id.</param>
    /// <param name="request">The fields to update.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPut("{id:int}", Name = "UpdateMechanic")]
    public async Task<IActionResult> UpdateMechanic(
        [FromRoute] int id,
        [FromBody] UpdateMechanicRequest request,
        CancellationToken ct = default)
    {
        var mechanic = await _context.Mechanics.FindAsync([id], ct);

        if (mechanic is null)
            return NotFound();

        mechanic.ApplyUpdate(request);
        await _context.SaveChangesAsync(ct);

        return Ok(new ApiResponse<UpdateMechanicResponse>
        {
            Data = mechanic.ToUpdateMechanicResponse(),
            Links = GetMechanicLinks(Url, id)
        });
    }

    /// <summary>
    /// Deletes a mechanic by id.
    /// </summary>
    /// <remarks>
    /// Unlike Domains/Categories/Publishers, deleting a mechanic only requires an authenticated
    /// user, not the Administrator role.
    /// </remarks>
    /// <param name="id">The mechanic's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpDelete("{id:int}", Name = "DeleteMechanicById")]
    public async Task<IActionResult> DeleteMechanicById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var mechanic = await _context.Mechanics.FindAsync([id], ct);

        if (mechanic is null)
            return NotFound();

        _context.Mechanics.Remove(mechanic);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    private static List<Link> GetMechanicLinks(IUrlHelper url, int id)
    {
        return new List<Link>
        {
            new Link
            {
                Href = url.Link("GetMechanicById", new { id })!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            },
            new Link
            {
                Href = url.Link("UpdateMechanic", new { id })!,
                Method = HttpMethod.Put.Method,
                Rel = "update"
            },
            new Link
            {
                Href = url.Link("DeleteMechanicById", new { id })!,
                Method = HttpMethod.Delete.Method,
                Rel = "delete"
            }
        };
    }
}

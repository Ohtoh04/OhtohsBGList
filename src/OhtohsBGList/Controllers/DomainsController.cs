using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using OhtohsBGList.Attributes;
using OhtohsBGList.Constants;
using OhtohsBGList.Contracts;
using OhtohsBGList.Contracts.Domains;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Models;
using OhtohsBGList.Mappings;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/domains")]
public class DomainsController(
    ILogger<DomainsController> logger,
    BgDbContext dbContext)
    : ControllerBase
{
    private readonly ILogger<DomainsController> _logger = logger;
    private readonly BgDbContext _context = dbContext;

    /// <summary>
    /// Gets a paged, sorted and optionally filtered list of domains.
    /// </summary>
    /// <param name="request">Paging, sorting and filtering parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <remarks>
    /// Validation is handled manually (see <see cref="ManualValidationFilterAttribute"/>): an invalid
    /// <see cref="PagedRequest{T}.PageSize"/> alone returns 501 Not Implemented rather than the usual 400.
    /// </remarks>
    [ManualValidationFilter]
    [HttpGet(Name = "GetDomains")]
    public async Task<IActionResult> GetDomains(
        [FromQuery] PagedRequest<Domain> request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            var pageSizeInvalid = ModelState.Any(kvp =>
                kvp.Key.Contains(nameof(PagedRequest<Domain>.PageSize)) && kvp.Value?.Errors.Count > 0);

            if (pageSizeInvalid)
                return StatusCode(StatusCodes.Status501NotImplemented);

            return ValidationProblem(ModelState);
        }

        var query = _context.Domains.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.FilterQuery))
            query = query.Where(d => d.Name.Contains(request.FilterQuery));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy($"{request.SortColumn} {request.SortOrder}")
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var pagedResponse = new PagedResponse<Domain>(items, request.PageNumber, request.PageSize, totalCount);

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
                Href = Url.Link("GetDomains", RouteValuesForPage(request.PageNumber))!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            }
        };

        if (pagedResponse.HasNextPage)
            links.Add(new Link
            {
                Href = Url.Link("GetDomains", RouteValuesForPage(request.PageNumber + 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "next"
            });

        if (pagedResponse.HasPreviousPage)
            links.Add(new Link
            {
                Href = Url.Link("GetDomains", RouteValuesForPage(request.PageNumber - 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "prev"
            });

        return Ok(new ApiResponse<PagedResponse<Domain>> { Data = pagedResponse, Links = links });
    }

    /// <summary>
    /// Gets a single domain by id.
    /// </summary>
    /// <param name="id">The domain's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [HttpGet("{id:int}", Name = "GetDomainById")]
    public async Task<IActionResult> GetDomainById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var domain = await _context.Domains.FindAsync([id], ct);

        if (domain is null)
            return NotFound();

        return Ok(new ApiResponse<Domain> { Data = domain, Links = GetDomainLinks(Url, id) });
    }

    /// <summary>
    /// Creates a new domain.
    /// </summary>
    /// <param name="request">The domain's name, notes and flags.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPost(Name = "CreateDomain")]
    public async Task<IActionResult> CreateDomain(
        [FromBody] CreateDomainRequest request,
        CancellationToken ct = default)
    {
        var domain = request.ToDomain();

        await _context.Domains.AddAsync(domain, ct);
        await _context.SaveChangesAsync(ct);

        var response = new ApiResponse<CreateDomainResponse>
        {
            Data = domain.ToCreateDomainResponse(),
            Links = GetDomainLinks(Url, domain.Id)
        };

        return CreatedAtAction(nameof(GetDomainById), new { id = domain.Id }, response);
    }

    /// <summary>
    /// Updates an existing domain.
    /// </summary>
    /// <param name="id">The domain's id.</param>
    /// <param name="request">The fields to update.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPut("{id:int}", Name = "UpdateDomain")]
    public async Task<IActionResult> UpdateDomain(
        [FromRoute] int id,
        [FromBody] UpdateDomainRequest request,
        CancellationToken ct = default)
    {
        var domain = await _context.Domains.FindAsync([id], ct);

        if (domain is null)
            return NotFound();

        domain.ApplyUpdate(request);
        await _context.SaveChangesAsync(ct);

        return Ok(new ApiResponse<UpdateDomainResponse>
        {
            Data = domain.ToUpdateDomainResponse(),
            Links = GetDomainLinks(Url, id)
        });
    }

    /// <summary>
    /// Deletes a domain by id.
    /// </summary>
    /// <param name="id">The domain's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Administrator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpDelete("{id:int}", Name = "DeleteDomainById")]
    public async Task<IActionResult> DeleteDomainById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var domain = await _context.Domains.FindAsync([id], ct);

        if (domain is null)
            return NotFound();

        _context.Domains.Remove(domain);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    private static List<Link> GetDomainLinks(IUrlHelper url, int id)
    {
        return new List<Link>
        {
            new Link
            {
                Href = url.Link("GetDomainById", new { id })!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            },
            new Link
            {
                Href = url.Link("UpdateDomain", new { id })!,
                Method = HttpMethod.Put.Method,
                Rel = "update"
            },
            new Link
            {
                Href = url.Link("DeleteDomainById", new { id })!,
                Method = HttpMethod.Delete.Method,
                Rel = "delete"
            }
        };
    }
}

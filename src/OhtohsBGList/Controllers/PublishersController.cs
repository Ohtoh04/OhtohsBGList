using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using OhtohsBGList.Constants;
using OhtohsBGList.Contracts;
using OhtohsBGList.Contracts.Publishers;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Models;
using OhtohsBGList.Mappings;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/publishers")]
public class PublishersController(
    ILogger<PublishersController> logger,
    BgDbContext dbContext)
    : ControllerBase
{
    private readonly ILogger<PublishersController> _logger = logger;
    private readonly BgDbContext _context = dbContext;

    /// <summary>
    /// Gets a paged, sorted and optionally filtered list of publishers.
    /// </summary>
    /// <param name="request">Paging, sorting and filtering parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    [HttpGet(Name = "GetPublishers")]
    public async Task<IActionResult> GetPublishers(
        [FromQuery] PagedRequest<Publisher> request,
        CancellationToken ct = default)
    {
        var query = _context.Publishers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.FilterQuery))
            query = query.Where(p => p.Name.Contains(request.FilterQuery));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy($"{request.SortColumn} {request.SortOrder}")
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var pagedResponse = new PagedResponse<Publisher>(items, request.PageNumber, request.PageSize, totalCount);

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
                Href = Url.Link("GetPublishers", RouteValuesForPage(request.PageNumber))!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            }
        };

        if (pagedResponse.HasNextPage)
            links.Add(new Link
            {
                Href = Url.Link("GetPublishers", RouteValuesForPage(request.PageNumber + 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "next"
            });

        if (pagedResponse.HasPreviousPage)
            links.Add(new Link
            {
                Href = Url.Link("GetPublishers", RouteValuesForPage(request.PageNumber - 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "prev"
            });

        return Ok(new ApiResponse<PagedResponse<Publisher>> { Data = pagedResponse, Links = links });
    }

    /// <summary>
    /// Gets a single publisher by id.
    /// </summary>
    /// <param name="id">The publisher's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [HttpGet("{id:int}", Name = "GetPublisherById")]
    public async Task<IActionResult> GetPublisherById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var publisher = await _context.Publishers.FindAsync([id], ct);

        if (publisher is null)
            return NotFound();

        return Ok(new ApiResponse<Publisher> { Data = publisher, Links = GetPublisherLinks(Url, id) });
    }

    /// <summary>
    /// Creates a new publisher.
    /// </summary>
    /// <param name="request">The publisher's name.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPost(Name = "CreatePublisher")]
    public async Task<IActionResult> CreatePublisher(
        [FromBody] CreatePublisherRequest request,
        CancellationToken ct = default)
    {
        var publisher = request.ToPublisher();

        await _context.Publishers.AddAsync(publisher, ct);
        await _context.SaveChangesAsync(ct);

        var response = new ApiResponse<CreatePublisherResponse>
        {
            Data = publisher.ToCreatePublisherResponse(),
            Links = GetPublisherLinks(Url, publisher.Id)
        };

        return CreatedAtAction(nameof(GetPublisherById), new { id = publisher.Id }, response);
    }

    /// <summary>
    /// Updates an existing publisher.
    /// </summary>
    /// <param name="id">The publisher's id.</param>
    /// <param name="request">The fields to update.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPut("{id:int}", Name = "UpdatePublisher")]
    public async Task<IActionResult> UpdatePublisher(
        [FromRoute] int id,
        [FromBody] UpdatePublisherRequest request,
        CancellationToken ct = default)
    {
        var publisher = await _context.Publishers.FindAsync([id], ct);

        if (publisher is null)
            return NotFound();

        publisher.ApplyUpdate(request);
        await _context.SaveChangesAsync(ct);

        return Ok(new ApiResponse<UpdatePublisherResponse>
        {
            Data = publisher.ToUpdatePublisherResponse(),
            Links = GetPublisherLinks(Url, id)
        });
    }

    /// <summary>
    /// Deletes a publisher by id.
    /// </summary>
    /// <param name="id">The publisher's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Administrator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpDelete("{id:int}", Name = "DeletePublisherById")]
    public async Task<IActionResult> DeletePublisherById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var publisher = await _context.Publishers.FindAsync([id], ct);

        if (publisher is null)
            return NotFound();

        _context.Publishers.Remove(publisher);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    private static List<Link> GetPublisherLinks(IUrlHelper url, int id)
    {
        return new List<Link>
        {
            new Link
            {
                Href = url.Link("GetPublisherById", new { id })!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            },
            new Link
            {
                Href = url.Link("UpdatePublisher", new { id })!,
                Method = HttpMethod.Put.Method,
                Rel = "update"
            },
            new Link
            {
                Href = url.Link("DeletePublisherById", new { id })!,
                Method = HttpMethod.Delete.Method,
                Rel = "delete"
            }
        };
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using OhtohsBGList.Constants;
using OhtohsBGList.Contracts;
using OhtohsBGList.Contracts.Categories;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Models;
using OhtohsBGList.Mappings;

namespace OhtohsBGList.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
public class CategoriesController(
    ILogger<CategoriesController> logger,
    BgDbContext dbContext)
    : ControllerBase
{
    private readonly ILogger<CategoriesController> _logger = logger;
    private readonly BgDbContext _context = dbContext;

    /// <summary>
    /// Gets a paged, sorted and optionally filtered list of categories.
    /// </summary>
    /// <param name="request">Paging, sorting and filtering parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    [HttpGet(Name = "GetCategories")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] PagedRequest<Category> request,
        CancellationToken ct = default)
    {
        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.FilterQuery))
            query = query.Where(c => c.Name.Contains(request.FilterQuery));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy($"{request.SortColumn} {request.SortOrder}")
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var pagedResponse = new PagedResponse<Category>(items, request.PageNumber, request.PageSize, totalCount);

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
                Href = Url.Link("GetCategories", RouteValuesForPage(request.PageNumber))!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            }
        };

        if (pagedResponse.HasNextPage)
            links.Add(new Link
            {
                Href = Url.Link("GetCategories", RouteValuesForPage(request.PageNumber + 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "next"
            });

        if (pagedResponse.HasPreviousPage)
            links.Add(new Link
            {
                Href = Url.Link("GetCategories", RouteValuesForPage(request.PageNumber - 1))!,
                Method = HttpMethod.Get.Method,
                Rel = "prev"
            });

        return Ok(new ApiResponse<PagedResponse<Category>> { Data = pagedResponse, Links = links });
    }

    /// <summary>
    /// Gets a single category by id.
    /// </summary>
    /// <param name="id">The category's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [HttpGet("{id:int}", Name = "GetCategoryById")]
    public async Task<IActionResult> GetCategoryById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var category = await _context.Categories.FindAsync([id], ct);

        if (category is null)
            return NotFound();

        return Ok(new ApiResponse<Category> { Data = category, Links = GetCategoryLinks(Url, id) });
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="request">The category's name.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPost(Name = "CreateCategory")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = request.ToCategory();

        await _context.Categories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);

        var response = new ApiResponse<CreateCategoryResponse>
        {
            Data = category.ToCreateCategoryResponse(),
            Links = GetCategoryLinks(Url, category.Id)
        };

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, response);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category's id.</param>
    /// <param name="request">The fields to update.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Moderator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpPut("{id:int}", Name = "UpdateCategory")]
    public async Task<IActionResult> UpdateCategory(
        [FromRoute] int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = await _context.Categories.FindAsync([id], ct);

        if (category is null)
            return NotFound();

        category.ApplyUpdate(request);
        await _context.SaveChangesAsync(ct);

        return Ok(new ApiResponse<UpdateCategoryResponse>
        {
            Data = category.ToUpdateCategoryResponse(),
            Links = GetCategoryLinks(Url, id)
        });
    }

    /// <summary>
    /// Deletes a category by id.
    /// </summary>
    /// <param name="id">The category's id.</param>
    /// <param name="ct">A cancellation token.</param>
    [Authorize(Roles = RoleNames.Administrator)]
    [ResponseCache(CacheProfileName = "NoCache")]
    [HttpDelete("{id:int}", Name = "DeleteCategoryById")]
    public async Task<IActionResult> DeleteCategoryById(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var category = await _context.Categories.FindAsync([id], ct);

        if (category is null)
            return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    private static List<Link> GetCategoryLinks(IUrlHelper url, int id)
    {
        return new List<Link>
        {
            new Link
            {
                Href = url.Link("GetCategoryById", new { id })!,
                Method = HttpMethod.Get.Method,
                Rel = "self"
            },
            new Link
            {
                Href = url.Link("UpdateCategory", new { id })!,
                Method = HttpMethod.Put.Method,
                Rel = "update"
            },
            new Link
            {
                Href = url.Link("DeleteCategoryById", new { id })!,
                Method = HttpMethod.Delete.Method,
                Rel = "delete"
            }
        };
    }
}

using System.ComponentModel.DataAnnotations;
using OhtohsBGList.Attributes;

namespace OhtohsBGList.Contracts;

/// <summary>
/// Generic paging, sorting and filtering parameters for list endpoints.
/// </summary>
/// <typeparam name="T">The entity type being listed, used to validate <see cref="SortColumn"/> against its properties.</typeparam>
public class PagedRequest<T> : IValidatableObject
{
    /// <summary>
    /// The 1-based page number to return. Defaults to 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// The number of items per page. Defaults to 10.
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// The property name to sort by. Defaults to "Name".
    /// </summary>
    public string SortColumn { get; set; } = "Name";

    /// <summary>
    /// The sort direction, "ASC" or "DESC". Defaults to "ASC".
    /// </summary>
    public string SortOrder { get; set; } = "ASC";

    /// <summary>
    /// An optional case-insensitive substring filter applied to the entity's Name.
    /// </summary>
    public string? FilterQuery { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var columnResult = new SortColumnValidatorAttribute(typeof(T))
            .GetValidationResult(SortColumn, validationContext);
        if (columnResult is not null)
            yield return new ValidationResult(columnResult.ErrorMessage, [nameof(SortColumn)]);

        var orderResult = new SortOrderValidationAttribute()
            .GetValidationResult(SortOrder, validationContext);
        if (orderResult is not null)
            yield return new ValidationResult(orderResult.ErrorMessage, [nameof(SortOrder)]);
    }
}

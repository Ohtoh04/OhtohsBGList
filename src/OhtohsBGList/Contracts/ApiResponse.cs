namespace OhtohsBGList.Contracts;

/// <summary>
/// Generic API response envelope containing data, links, and metadata.
/// </summary>
/// <typeparam name="T">Type of the result payload.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// The actual response data (nullable for error scenarios).
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// HATEOAS links related to the current resource.
    /// </summary>
    public List<Link> Links { get; set; } = [];

    /// <summary>
    /// Arbitrary response details.
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = [];
}

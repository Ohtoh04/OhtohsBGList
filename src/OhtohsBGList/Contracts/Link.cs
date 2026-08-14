namespace OhtohsBGList.Contracts;

/// <summary>
/// Represents a HATEOAS link.
/// </summary>
public class Link
{
    /// <summary>
    /// Gets or sets the target URI of the link.
    /// </summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the link relation type (e.g., "self", "next", "edit").
    /// </summary>
    public string Rel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method to be used when accessing the link.
    /// The default value is "GET".
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Gets or sets the media type (MIME type) of the representation returned by the link.
    /// This property is optional and may be <c>null</c>.
    /// </summary>
    public string? Type { get; set; }
}

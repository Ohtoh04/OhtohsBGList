namespace OhtohsBGList.Data.Models;

/// <summary>
/// Implemented by entities that carry <see cref="CreatedDate"/>/<see cref="LastModifiedDate"/> audit columns,
/// so <see cref="Interceptors.SetAuditInterceptor"/> can stamp them regardless of entity type.
/// </summary>
public interface IAuditable
{
    DateTime CreatedDate { get; set; }
    DateTime LastModifiedDate { get; set; }
}

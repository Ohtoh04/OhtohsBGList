using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data.Interceptors;

public class SetAuditInterceptor : ISaveChangesInterceptor
{
    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetAuditProperties(eventData.Context);
        return result;
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
       DbContextEventData eventData,
       InterceptionResult<int> result,
       CancellationToken cancellationToken = default)
    {
        SetAuditProperties(eventData.Context);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    private static void SetAuditProperties(DbContext? context)
    {
        if (context is null) return;

        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<BoardGame>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedDate == default)
                        entry.Entity.CreatedDate = utcNow;

                    if (!(entry.Entity.LastModifiedDate != default))
                        entry.Entity.LastModifiedDate = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedDate = utcNow;
                    break;
            }
        }
    }
}

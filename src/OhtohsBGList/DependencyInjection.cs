using Microsoft.EntityFrameworkCore;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Interceptors;
using OhtohsBGList.Options;

namespace OhtohsBGList;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection serviceCollection, Action<ApplicationServicesOptions> configure)
    {
        var cfg = new ApplicationServicesOptions();
        configure(cfg);

        if (string.IsNullOrWhiteSpace(cfg.SQLiteConnectionString))
        {
            throw new InvalidOperationException(
                $"{nameof(ApplicationServicesOptions.SQLiteConnectionString)} must be configured.");
        }

        serviceCollection.AddDbContext<BgDbContext>(options =>
        {
            options.UseSqlite(cfg.SQLiteConnectionString)
                .AddInterceptors(new SetAuditInterceptor());
        });

        return serviceCollection;
    }
}

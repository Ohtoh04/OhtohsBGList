using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OhtohsBGList.Constants;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Interceptors;
using OhtohsBGList.Data.Models;
using OhtohsBGList.Options;
using OhtohsBGList.Services;

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

        serviceCollection.AddSingleton<IEmailSender<ApiUser>, SmtpEmailSender>();

        return serviceCollection;
    }

    public static IServiceCollection AddApplicationOptions(
        this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<SmtpOptions>(
            configuration.GetSection(ConfigurationKeys.SmtpSection));

        return serviceCollection;
    }
}

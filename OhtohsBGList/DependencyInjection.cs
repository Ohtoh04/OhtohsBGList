using Microsoft.EntityFrameworkCore;
using OhtohsBGList.Data;

namespace OhtohsBGList;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection serviceCollection, Action<ApplicationServicesConfiguration> configure) // TODO: THINK OF CONFIGURE NAMING
    {
        var cfg = new ApplicationServicesConfiguration()
        { SQLiteConnectionString = "asdasd" }; // TODO: THINK OF HOW TO MAKE IT REQUIRED BUT AVAILABLE TO INSTANTIATION

        configure(cfg);

        serviceCollection.AddDbContext<BGDbContext>(options =>
        {
            options.UseSqlite("asdasd"); // TODO MAKE A PROPER CONFIGURING ACTION
        });

        return serviceCollection;
    }
}

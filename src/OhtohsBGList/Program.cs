using System.Security.Claims;
using Asp.Versioning;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeoSmart.Caching.Sqlite;
using OhtohsBGList;
using OhtohsBGList.Constants;
using OhtohsBGList.Data;
using OhtohsBGList.Data.Models;
using OhtohsBGList.Endpoints;
using OhtohsBGList.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration.GetValue<string[]>("AllowedHosts") ?? []);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddResponseCaching();
builder.Services.ConfigureOptions<SwaggerConfiguration>();

builder.Services.AddApplicationServices(options =>
{
    options.SQLiteConnectionString = builder.Configuration.GetConnectionString("BgDbContext");
});
builder.Services.AddApplicationOptions(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ModeratorWithMobilePhone", policy =>
        policy
            .RequireClaim(ClaimTypes.Role, RoleNames.Moderator)
            .RequireClaim(ClaimTypes.MobilePhone));
});

builder.Services.AddIdentityApiEndpoints<ApiUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
}).AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<BgDbContext>()
  .AddDefaultTokenProviders()
  .AddClaimsPrincipalFactory<ApiUserClaimsPrincipalFactory>();

builder.Services.AddSqliteCache(options =>
{
    var cacheDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "cache");
    Directory.CreateDirectory(cacheDirectory);

    options.CachePath = builder.Configuration["SqlCache:Path"]
        ?? Path.Combine(cacheDirectory, "boardgames-cache.db");
});

var app = builder.Build();

// Check if the DB was migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BgDbContext>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
        throw new Exception($"Database is not fully migrated for {nameof(BgDbContext)}.");
}

app.AddErrorEndpoint();
app.MapGroup("/api/auth").MapIdentityApi<ApiUser>();
app.AddDebugEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseCors();

app.UseResponseCaching();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

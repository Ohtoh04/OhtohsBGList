using System.Security.Claims;
using Asp.Versioning;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        x => $"The value '{x}' is invalid.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        x => $"The field {x} must be a number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "A value is required.");

    options.CacheProfiles.Add("NoCache", new CacheProfile { NoStore = true });
    options.CacheProfiles.Add("Any-60", new CacheProfile { Location = ResponseCacheLocation.Any, Duration = 60 });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration.GetValue<string[]>("AllowedHosts") ?? []);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
    options.AddPolicy("GetOnly", cfg =>
    {
        cfg.AllowAnyOrigin();
        cfg.AllowAnyHeader();
        cfg.WithMethods(HttpMethod.Get.Method);
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

using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OhtohsBGList;

public class SwaggerConfiguration(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfo(description));
        }
    }

    private static OpenApiInfo CreateInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "OhtohsBGList API",
            Version = description.ApiVersion.ToString(),
            Description = "DESCRIPTION HERE"
        };

        if (description.IsDeprecated)
        {
            info.Description += " This version has been deprecated.";
        }

        return info;
    }
}
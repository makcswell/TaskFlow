using Asp.Versioning.ApiExplorer;
using Ebceys.Infrastructure.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ebceys.Infrastructure.Helpers.Swagger;

/// <summary>
///     Configures Swagger/OpenAPI document generation options for each discovered API version.
///     Generates a separate Swagger document per API version with the service name and description
///     from <see cref="ServiceApiInfo" />.
/// </summary>
/// <param name="provider">The API version description provider.</param>
/// <param name="serviceApiInfo">The service API metadata (name and description).</param>
[PublicAPI]
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IOptions<ServiceApiInfo> serviceApiInfo)
    : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    ///     The api info. Will use instead of default if specified.
    /// </summary>
    public static OpenApiInfo? ApiInfo { get; set; }

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                CreateInfoForApiVersion(description));
        }
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = ApiInfo ?? new OpenApiInfo
        {
            Title = serviceApiInfo.Value.ServiceName,
            Version = description.ApiVersion.ToString(),
            Description = serviceApiInfo.Value.Description
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}
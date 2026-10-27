using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using EBCEYS.ContainersEnvironment.HealthChecks.Extensions;
using Ebceys.Infrastructure.ControllerFilters;
using Ebceys.Infrastructure.Extensions;
using Ebceys.Infrastructure.HealthChecks;
using Ebceys.Infrastructure.Helpers;
using Ebceys.Infrastructure.Helpers.Jwt;
using Ebceys.Infrastructure.Helpers.Swagger;
using Ebceys.Infrastructure.Interfaces;
using Ebceys.Infrastructure.Middlewares;
using Ebceys.Infrastructure.Models;
using Ebceys.Infrastructure.Options;
using HealthChecks.ApplicationStatus.DependencyInjection;
using JetBrains.Annotations;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Prometheus;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable MemberCanBePrivate.Global

namespace Ebceys.Infrastructure;

/// <summary>
///     Abstract base class for application startup configuration. Provides default configurations for
///     logging (Serilog), Swagger/OpenAPI, health checks, Prometheus metrics, API versioning, JWT authentication,
///     FluentValidation, and standard middlewares. Derive from this class to configure your service.
/// </summary>
/// <param name="configuration">The application configuration.</param>
[PublicAPI]
public abstract class ExtraStartupBase(IConfiguration configuration) : IStartupBase
{
    private const string JwtOptionsConfigPath = "JwtOptions";

    /// <summary>
    ///     Indicates that server should use authentication.
    /// </summary>
    protected virtual bool UseAuthentication { get; init; }

    /// <summary>
    ///     Indicates that jwt should be proxied and not be configured at the service.
    /// </summary>
    protected virtual bool ProxyToken { get; init; } = true;

    /// <summary>
    ///     The health check server port.
    /// </summary>
    protected virtual int? HealthCheckPort { get; init; }

    /// <summary>
    ///     The service api info.
    /// </summary>
    protected abstract ServiceApiInfo ServiceApiInfo { get; init; }

    /// <summary>
    ///     The configuration.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected IConfiguration Configuration { get; } = configuration;

    /// <summary>
    ///     The http context logging options.
    /// </summary>
    protected virtual Action<HttpLoggingOptions>? HttpContextLogging { get; }

    /// <summary>
    ///     Configures the <paramref name="services" />.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSerilog((_, loggerConf)
            => loggerConf.ReadFrom.Configuration(Configuration));

        services.AddOptions<HttpLoggingOptions>().Configure(opts => { HttpContextLogging?.Invoke(opts); });

        services.AddControllers(opts =>
        {
            opts.Filters.Add<ApiExceptionFilter>();
            ConfigureFilters(opts.Filters);
        });
        services.AddSingleton<IOptions<ServiceApiInfo>>(new OptionsWrapper<ServiceApiInfo>(ServiceApiInfo));
        services.AddHttpContextAccessor();

        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationRulesToSwagger();

        services.AddRouting(opts => { opts.LowercaseUrls = true; });
        services.AddProblemDetails(opts =>
        {
            opts.CustomizeProblemDetails = ctx =>
            {
                ctx.HttpContext.Response.StatusCode =
                    ctx.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
                ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
                ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
                ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
            };
        });
        services.Configure<FormOptions>(ConfigureFormOptions);

        if (UseAuthentication)
        {
            services.AddAuthorization();
            ConfigureAuthentication(services);
        }

        ConfigureSwagger(services);


        var healthChecksBuilder = services.ConfigureHealthChecks();
        healthChecksBuilder?.AddApplicationStatus();
        healthChecksBuilder?.AddApplicationInsightsPublisher();

        services.AddSingleton<ICommandExecutor, CommandExecutor>();
        services.AddScoped<IScopedCommandExecutor, ScopedCommandExecutor>();
        ServicesConfiguration(services);
        ConfigureHealthChecks(healthChecksBuilder);
        healthChecksBuilder?.AddRabbitMqHealthChecks();
        healthChecksBuilder?.AddNpgsqlHealthChecks();
    }

    /// <summary>
    ///     Configures the application request pipeline including routing, logging, CORS, Prometheus metrics,
    ///     health checks, authentication, Swagger UI, and custom middlewares.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    /// <param name="env">The hosting environment information.</param>
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UsePathBase(ServiceApiInfo.BaseAddress);

        app.UseRouting();

        app.UseRequestLogging();

        app.UseStatusCodePages(async ctx =>
        {
            var problemDetails = ctx.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
            var problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = ctx.HttpContext,
                ProblemDetails = ProblemDetails.CreateFromResponse(ctx.HttpContext.Response)
            };
            await problemDetails.TryWriteAsync(problemDetailsContext);
        });

        app.UseExceptionCatcher();

        app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

        app.UseMetricServer();
        app.UseHttpMetrics();
        app.UseRequestMetrics();

        if (HealthCheckPort is null)
        {
            app.ConfigureHealthChecks();
        }
        else
        {
            app.ConfigureHealthChecks(HealthCheckPort.Value);
        }

        if (UseAuthentication)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseSwagger();
        app.UseSwaggerUI(opts =>
        {
            var descriptions = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var apiVersion in descriptions.ApiVersionDescriptions)
            {
                opts.SwaggerEndpoint($"{ServiceApiInfo.BaseAddress}/swagger/{apiVersion.GroupName}/swagger.json",
                    apiVersion.GroupName);
            }

            opts.DocumentTitle = ServiceApiInfo.ServiceName;
        });


        ConfigureMiddlewares(app, env);

        app.UseEndpoints(endpoints =>
        {
            ConfigureEndpoints(endpoints);
            endpoints.MapControllers();
        });
    }

    /// <summary>
    ///     Use it to configure your custom endpoints. Called during application configuration after all middlewares and before
    ///     controller endpoints are mapped.
    /// </summary>
    /// <param name="endpoints">The endpoints.</param>
    protected virtual void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
    }

    /// <summary>
    ///     Use it to configure your custom filters.
    /// </summary>
    /// <param name="filters">The filters.</param>
    protected virtual void ConfigureFilters(FilterCollection filters)
    {
    }

    /// <summary>
    ///     Use it to configure your custom Swagger/OpenAPI options. Called during service configuration after default options
    ///     are set.
    /// </summary>
    /// <param name="options">The swagger gen options.</param>
    protected virtual void ConfigureSwagger(SwaggerGenOptions options)
    {
    }

    /// <summary>
    ///     Use it to configure form options, e.g. for file uploads.
    /// </summary>
    /// <param name="options">The form options.</param>
    protected virtual void ConfigureFormOptions(FormOptions options)
    {
        options.ValueLengthLimit = int.MaxValue;
        options.MultipartBodyLengthLimit = int.MaxValue;
    }

    /// <summary>
    ///     Configures the health checks.
    /// </summary>
    /// <param name="builder">The healthcheck builder.</param>
    /// <returns>The enhanced instance of <paramref name="builder" />.</returns>
    protected virtual IHealthChecksBuilder? ConfigureHealthChecks(IHealthChecksBuilder? builder)
    {
        return builder;
    }

    /// <summary>
    ///     Service configuration.
    /// </summary>
    /// <param name="services">The service configuration.</param>
    protected abstract void ServicesConfiguration(IServiceCollection services);

    /// <summary>
    ///     Configures custom middlewares for the application pipeline. Called before controller endpoints are mapped.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The hosting environment.</param>
    protected abstract void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env);

    private void ConfigureAuthentication(IServiceCollection services)
    {
        var builder = services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtGenerator.AuthSchema;
            opts.DefaultScheme = JwtGenerator.AuthSchema;
            opts.DefaultChallengeScheme = JwtGenerator.AuthSchema;
        });

        if (ProxyToken)
        {
            builder.AddJwtBearer();
            return;
        }

        services.Configure<JwtOptions>(Configuration.GetSection(JwtOptionsConfigPath));
        var jwtOpts = Configuration.GetSection(JwtOptionsConfigPath).Get<JwtOptions>()
                      ?? throw new ApplicationException("Can not find jwt options!");
        builder.AddJwtBearer(cfg =>
        {
            cfg.RequireHttpsMetadata = false;
            cfg.SaveToken = true;
            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtOpts.Issuer,
                ValidAudience = jwtOpts.Audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = jwtOpts.TokenTimeToLive != null,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Convert.FromBase64String(jwtOpts.Base64Key))
            };
        });
        services.TryAddSingleton<IJwtGenerator, JwtGenerator>();
    }

    private void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opts =>
        {
            opts.DocInclusionPredicate((docName, apiDesc) => apiDesc.GroupName == docName);
            if (UseAuthentication)
            {
                const string schemeId = "bearer";
                opts.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = schemeId,
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                opts.AddSecurityRequirement(document =>
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(schemeId, document)] = []
                    });
            }

            opts.IncludeXmlComments(Assembly.GetExecutingAssembly(), true);
            ConfigureSwagger(opts);
        });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader()
            );
        }).AddMvc().AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.SubstituteApiVersionInUrl = true;
        });
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    }
}
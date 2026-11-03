using Ebceys.Infrastructure;
using Ebceys.Infrastructure.DatabaseRegistration;
using Ebceys.Infrastructure.Extensions;
using Ebceys.Infrastructure.Models;
using TasksService.Commands;
using TasksService.Data;
using TasksService.Routes;

namespace TasksService;

/// <summary>
/// TasksService application configuration.
/// </summary>
public class Startup : ExtraStartupBase
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    public Startup(IConfiguration configuration) : base(configuration)
    {
    }

    /// <inheritdoc />
    protected override bool UseAuthentication { get; init; } = true;

    /// <summary>
    /// Service metadata.
    /// </summary>
    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new(
            ServiceName: RoutesDictionary.ServiceId,
            BaseAddress: RoutesDictionary.TaskFlowStartup.BaseAddress,
            Description: RoutesDictionary.TaskFlowStartup.Description);

    /// <summary>
    /// Services registration.
    /// </summary>
    /// <param name="services">Services collection.</param>
    protected override void ServicesConfiguration(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        services.RegisterDbContext<AppDbContext>(opts =>
        {
            opts.ConnectionString = connectionString;
            opts.MigrateDb = true;
        });
        services.AddScopedCommand<ValidateTokenCommand, IHeaderDictionary, bool>();
    }

    /// <summary>
    /// Middleware configuration.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="env">Host environment.</param>
    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}
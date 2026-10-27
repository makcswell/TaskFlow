using Ebceys.Infrastructure.AuthorizationTestApplication.Client;
using Ebceys.Infrastructure.DatabaseRegistration;
using Ebceys.Infrastructure.Extensions;
using Ebceys.Infrastructure.Helpers.Sequences;
using Ebceys.Infrastructure.HttpClient.ClientRegistration;
using Ebceys.Infrastructure.Middlewares;
using Ebceys.Infrastructure.Models;
using Ebceys.Infrastructure.Options;
using Ebceys.Infrastructure.RabbitMq;
using Ebceys.Infrastructure.TestApplication.BoundedContext;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using Ebceys.Infrastructure.TestApplication.Commands;
using Ebceys.Infrastructure.TestApplication.DaL;
using Ebceys.Infrastructure.TestApplication.Validators;

namespace Ebceys.Infrastructure.TestApplication;

public class Program
{
    public static async Task Main(string[] args)
    {
        await TestAppInitiator.InitiateApp(args).RunAsync();
    }
}

public static class TestAppInitiator
{
    public static ConfiguredApp InitiateApp(params string[] args)
    {
        var builder = WebApplicationBase<Startup>.Create(args);
        return builder.Build(args);
    }
}

public class Startup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override int? HealthCheckPort { get; init; } = 8080;
    protected override bool ProxyToken { get; init; } = true;
    protected override bool UseAuthentication { get; init; } = true;

    protected override Action<HttpLoggingOptions>? HttpContextLogging { get; } = opts =>
    {
        opts.LogLevelToLogBodies = LogLevel.Trace;
        opts.PathContainsExcludeLogging = [RoutesDictionary.TestControllerV1.Methods.GetToken];
        opts.PathEndExcludeLogging = [RoutesDictionary.TestControllerV1.Methods.PostBody];
        opts.PathStartExcludeLogging =
        [
            $"{RoutesDictionary.TestControllerV1.BaseRoute.Replace(RoutesDictionary.BasePath, "")}/{RoutesDictionary.TestControllerV1.Methods.GetQuery}"
        ];
    };

    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new("test-app", RoutesDictionary.BasePath, "TestApplication");

    protected override void ServicesConfiguration(IServiceCollection services)
    {
        services.RegisterDbContext<DataModelContext>(opts =>
        {
            opts.MigrateDb = true;
            opts.ConnectionString = Configuration.GetConnectionString("TestApplication");
        });

        services.AddScopedCommand<AddEntityCommand, AddEntityCommandContext, AddEntityCommandResult>();
        services.AddScopedCommand<GetEntitiesCommand, GetEntitiesCommandContext, GetEntitiesCommandResult>();
        services.AddScopedCommand<DeleteElementCommand, DeleteElementCommandContext, DeleteElementCommandResult>();
        services.AddScopedCommand<UpdateEntityCommand, UpdateEntityCommandContext, UpdateEntityCommandResult>();


        services.AddValidator<ChangeNameRequestValidator, ChangeNameRequest>();
        services.AddValidator<SomeBodyRequestValidator, SomeBodyRequest>();
        services.AddAtomicGenerators();

        var rabbitConfig = Configuration.GetRabbitMqConfiguration("rabbit");
        services.AddRabbitMqClient<IAppRabbitClient, AppRabbitClient>(rabbitConfig);

        services.AddClient<IAuthAppClient, AuthAppClient>()
            .FromConfiguration(Configuration, AuthorizationTestApplication.BoundedContext.RoutesDictionary.ServiceName)
            .AddAuthTokenFromHttpContextResolver()
            .Register();
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}
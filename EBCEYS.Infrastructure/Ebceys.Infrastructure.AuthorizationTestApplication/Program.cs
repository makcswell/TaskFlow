using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.AuthorizationTestApplication.Rabbit;
using Ebceys.Infrastructure.AuthorizationTestApplication.SchedulingJobs;
using Ebceys.Infrastructure.Models;
using Ebceys.Infrastructure.Options;
using Ebceys.Infrastructure.RabbitMq;
using Ebceys.Infrastructure.Scheduling;

namespace Ebceys.Infrastructure.AuthorizationTestApplication;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplicationBase<AuthorizationStartup>.Create(args);
        await builder.Build(args).RunAsync();
    }
}

public class AuthorizationStartup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new(RoutesDictionary.ServiceName, RoutesDictionary.BasePath, "TestAuthService");

    protected override bool UseAuthentication { get; init; } = true;
    protected override bool ProxyToken { get; init; } = false;

    protected override void ServicesConfiguration(IServiceCollection services)
    {
        var rabbitConfig = Configuration.GetRabbitMqConfiguration("rabbit");
        services.AddRabbitMqController<RabbitMqController>(rabbitConfig);
        services.AddSchedulingServices(true,
            async factory =>
            {
                await factory.ScheduleJobAsync<LogSomeJob>(Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1));
            });
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}
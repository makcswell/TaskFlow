using System.Diagnostics;
using Ebceys.Infrastructure.Middlewares;
using Ebceys.Infrastructure.Scheduling;
using Ebceys.Infrastructure.Tests.AppInitializer;
using Ebceys.Infrastructure.Tests.Helpers;
using Ebceys.Infrastructure.Tests.ScheduledTests.Helpers;
using EBCEYS.RabbitMQ.Configuration;
using Ebceys.Tests.Infrastructure.Helpers;
using Ebceys.Tests.Infrastructure.IntegrationTests;
using Ebceys.Tests.Infrastructure.IntegrationTests.ExternalServices.Containers;
using Testcontainers.PostgreSql;

namespace Ebceys.Infrastructure.Tests;

[SetUpFixture]
[NonParallelizable]
internal class AppTestContext
{
    private const string DbUser = "admin";
    private const string DbPassword = "password";
    private const string QueueName = "someQueue";
    private const string ExName = "someEx";
    private const string SectionBase = "rabbit";

    private static readonly string SolutionRelativePath = typeof(AppTestContext).Namespace!;

    private IDependencyInitializer<PostgreSqlContainer> _postgres;
    private RabbitMqInitializer _rabbit;

    public static AppTestClientContext AppContext { get; private set; }

    public static LogCatcherMiddleware<RequestLoggingMiddleware> AppLogCatcher { get; } = new();

    public static AuthAppTestClientContext AuthAppClientContext { get; private set; }


    [OneTimeSetUp]
    public async Task Setup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        _postgres = new PostgresInitializer(DbUser, DbPassword);
        var container = await _postgres.InitializeAsync();
        _rabbit = new RabbitMqInitializer(DbUser, DbPassword);
        var rabbit = await _rabbit.InitializeAsync();


        AuthAppClientContext = new AuthAppTestClientContext(sr =>
            sr.AddSingleton<Func<DelegatingHandler>>(() => new TestRoutingMessageHandler()));
        AuthAppClientContext.Initialize(conf =>
        {
            conf.UseProductionAppSettings = true;
            conf.SolutionRelativePath = SolutionRelativePath;
            conf.BuilderConfiguration = builder =>
            {
                builder.AddInMemoryConfig("JwtOptions:Issuer", "vitaliy");
                builder.AddInMemoryConfig("JwtOptions:Audience", "vitaliy");
                builder.AddInMemoryConfig("JwtOptions:Base64Key", new EbRandomizer().HexString(64));
                builder.AddRabbitMqConfig(rabbit, SectionBase, ExName, ExchangeTypes.Fanout, QueueName,
                    TimeSpan.FromSeconds(5));
            };
        });

        TestRoutingMessageHandler.RouteConfiguration.AddRoute(AuthAppClientContext);

        AppContext = new AppTestClientContext(sr =>
        {
            sr.AddSingleton<ILogger<RequestLoggingMiddleware>>(AppLogCatcher);
            sr.AddSchedulingServices(true, async factory =>
            {
                await factory.ScheduleJobAsync<ScheduledTestJob>(Guid.NewGuid().ToString(),
                    TimeSpan.FromMilliseconds(100));
            });
            sr.AddSingleton<Func<DelegatingHandler>>(() => new TestRoutingMessageHandler());
        });
        AppContext.Initialize(conf =>
        {
            conf.UseProductionAppSettings = true;
            conf.SolutionRelativePath = SolutionRelativePath;
            conf.BuilderConfiguration = builder =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:TestApplication", $"{container.GetConnectionString()};Database=dev-db" },
                    { "auth-service:ServiceUrl", AuthAppClientContext.BaseAddress }
                });
                builder.AddRabbitMqConfig(rabbit, SectionBase, ExName, ExchangeTypes.Fanout, QueueName,
                    TimeSpan.FromSeconds(5));
            };
        });

        TestRoutingMessageHandler.RouteConfiguration.AddRoute(AppContext);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        AuthAppClientContext.Teardown();
        AppContext.Teardown();

        await _postgres.TeardownAsync();
        await _rabbit.TeardownAsync();
    }
}
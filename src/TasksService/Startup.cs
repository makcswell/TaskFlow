using Ebceys.Infrastructure;
using Ebceys.Infrastructure.DatabaseRegistration;
using Ebceys.Infrastructure.Models;
using TasksService.Data;

namespace TasksService;

/// <summary>
/// Настройка приложения TasksService.
/// </summary>
public class Startup : ExtraStartupBase
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="configuration">Конфигурация приложения.</param>
    public Startup(IConfiguration configuration) : base(configuration)
    {
    }

    /// <inheritdoc />
    protected override bool UseAuthentication { get; init; } = true;

    /// <summary>
    /// Метаданные сервиса.
    /// </summary>
    protected override ServiceApiInfo ServiceApiInfo { get; init; } = 
        new(
            ServiceName: "TasksService",
            BaseAddress: "/tasks",
            Description: "Сервис управления задачами TaskFlow");

    /// <summary>
    /// Регистрация сервисов.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    protected override void ServicesConfiguration(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        services.RegisterDbContext<AppDbContext>(opts =>
        {
            opts.ConnectionString = connectionString;
            opts.MigrateDb = true;
        });
    }

    /// <summary>
    /// Настройка middleware.
    /// </summary>
    /// <param name="app">Построитель приложения.</param>
    /// <param name="env">Окружение хоста.</param>
    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}
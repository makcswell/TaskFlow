using Ebceys.Infrastructure;
using Ebceys.Infrastructure.Models;

namespace ShedulerService;

/// <summary>
///     The startup.
/// </summary>
/// <param name="configuration"></param>
public class Startup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    /// <summary>
    ///     ServiceApiInfo
    /// </summary>
    /// <exception cref="Exception"></exception>
    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new(
            ServiceName: "ShedulerService",
            BaseAddress: "/tasks",
            Description: "Сервис управления расписанием ");

    /// <summary>
    ///     Service Configure.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    protected override void ServicesConfiguration(IServiceCollection services)
    {
        //
        //
    }

    ///<summary>
    ///Middleware configure
    /// </summary>
    /// <param name="app">Построитель приложения.</param>
    /// <param name="env">Окружение хоста.</param>
    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}
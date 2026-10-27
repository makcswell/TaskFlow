using Ebceys.Infrastructure;
using Ebceys.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ShedulerService;

public class Startup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new(
            ServiceName: "ShedulerService", 
            BaseAddress: "/tasks", 
            Description: "Сервис управления расписанием ");

    protected override void ServicesConfiguration(IServiceCollection services)
    {
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}
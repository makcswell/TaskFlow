using Ebceys.Infrastructure;
using Ebceys.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TasksService;

public class Startup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override bool UseAuthentication { get; init; } = true;

    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new(
            ServiceName: "TasksService", 
            BaseAddress: "/tasks", 
            Description: "Сервис управления задачами TaskFlow");
    
    protected override void ServicesConfiguration(IServiceCollection services)
    {
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ebceys.Infrastructure.Interfaces;

/// <summary>
///     Internal interface that defines the contract for application startup configuration.
///     Implemented by <see cref="Infrastructure.ExtraStartupBase" /> to provide service and middleware configuration.
/// </summary>
internal interface IStartupBase
{
    /// <summary>
    ///     Configures the application services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    void ConfigureServices(IServiceCollection services);

    /// <summary>
    ///     Configures the application request pipeline and middlewares.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The hosting environment.</param>
    void Configure(IApplicationBuilder app, IHostEnvironment env);
}
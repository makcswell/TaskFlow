using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Ebceys.Infrastructure;

/// <summary>
///     Factory class for building and creating configured web applications using the specified startup class.
/// </summary>
/// <typeparam name="TStartup">The startup class derived from <see cref="ExtraStartupBase" />.</typeparam>
[PublicAPI]
public class WebApplicationBase<TStartup> where TStartup : ExtraStartupBase
{
    /// <summary>
    ///     Builds the application using <see cref="WebApplication.CreateBuilder(string[])" /> and configures
    ///     the web host with <typeparamref name="TStartup" />.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="configure">The action to configure <see cref="IHostBuilder"/>.</param>
    /// <returns>The new instance of <see cref="ConfiguredApp" /> ready to be run.</returns>
    public ConfiguredApp Build(string[] args, Action<IHostBuilder>? configure = null)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(x => x.UseStartup<TStartup>());

        configure?.Invoke(builder);

        return new ConfiguredApp(builder);
    }

    /// <summary>
    ///     Creates the new instance of <see cref="WebApplicationBase{TStartup}" />.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>The new instance of <see cref="WebApplicationBase{TStartup}" />.</returns>
    public static WebApplicationBase<TStartup> Create(params string[] args)
    {
        return new WebApplicationBase<TStartup>();
    }
}
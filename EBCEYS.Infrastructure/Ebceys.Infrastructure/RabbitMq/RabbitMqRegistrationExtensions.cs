using Ebceys.Infrastructure.HealthChecks;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Ebceys.Infrastructure.RabbitMq;

/// <summary>
///     Extension methods for registering RabbitMQ clients and smart controllers in the DI container.
///     Automatically registers health checks for all added RabbitMQ connections.
/// </summary>
[PublicAPI]
public static class RabbitMqRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the rabbitMq client to <paramref name="services" />.
        /// </summary>
        /// <param name="config">The rabbit mq configuration.</param>
        /// <param name="serializerSettings">The json serializer settings.</param>
        /// <typeparam name="TInterface">The client interface.</typeparam>
        /// <typeparam name="TImplementation">The client implementation.</typeparam>
        /// <returns>
        ///     <paramref name="services" />
        /// </returns>
        public IServiceCollection AddRabbitMqClient<TInterface, TImplementation>(RabbitMQConfiguration config,
            JsonSerializerSettings? serializerSettings = null)
            where TImplementation : EbRabbitMqClient, TInterface where TInterface : class
        {
            services.AddSingleton<TInterface, TImplementation>(sp =>
            {
                if (serializerSettings is null)
                {
                    return ActivatorUtilities.CreateInstance<TImplementation>(sp, config);
                }

                return ActivatorUtilities.CreateInstance<TImplementation>(sp, config, serializerSettings);
            });
            services.AddHostedService(sp =>
            {
                if (sp.GetRequiredService<TInterface>() is TImplementation instance)
                {
                    return instance;
                }

                throw new NotSupportedException(
                    $"The service provider does not implement {typeof(TImplementation).FullName}.");
            });
            HealthChecksCollectorService.Rabbits.Add(config.Factory);
            return services;
        }

        /// <summary>
        ///     Adds the <see cref="RabbitMQSmartControllerBase" /> to <paramref name="services" />.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="gZipSettings">The gzip settings.</param>
        /// <param name="serializerOptions">The serrializer options.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///     <paramref name="services" />
        /// </returns>
        public IServiceCollection AddRabbitMqController<T>(
            RabbitMQConfiguration configuration, GZipSettings? gZipSettings = null,
            JsonSerializerSettings? serializerOptions = null) where T : RabbitMQSmartControllerBase
        {
            services.AddSmartRabbitMQController<T>(configuration, gZipSettings, serializerOptions);
            HealthChecksCollectorService.Rabbits.Add(configuration.Factory);
            return services;
        }
    }
}
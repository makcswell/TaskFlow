using Ebceys.Infrastructure.Options;
using EBCEYS.RabbitMQ.Configuration;
using Ebceys.Tests.Infrastructure.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace Ebceys.Tests.Infrastructure.IntegrationTests.ExternalServices.Containers;

/// <summary>
///     Extension methods for configuring Testcontainers-based external dependencies (RabbitMQ, PostgreSQL)
///     in integration tests. Provides helpers to extract connection factories and inject configuration
///     into the test web host builder.
/// </summary>
[PublicAPI]
public static class ExternalDependenciesExtensions
{
    extension(RabbitMqContainer container)
    {
        /// <summary>
        ///     Gets the connection factory for <paramref name="container" />.
        /// </summary>
        /// <returns>The new instance of <see cref="IConnectionFactory" />.</returns>
        public IConnectionFactory GetConnectionFactory()
        {
            return new ConnectionFactory
            {
                Uri = new Uri(container.GetConnectionString())
            };
        }
    }

    extension(IWebHostBuilder builder)
    {
        /// <summary>
        ///     Adds the rabbit mq config to service configuration.
        /// </summary>
        /// <param name="container">The rabbit mq container.</param>
        /// <param name="sectionBase">The section in configuration name.</param>
        /// <param name="exName">The exchange name.</param>
        /// <param name="exType">The exchange type.</param>
        /// <param name="queueName">The queue name.</param>
        /// <param name="timeout">The timeout. Callback settings will apply only if timeout set.</param>
        public void AddRabbitMqConfig(RabbitMqContainer container, string sectionBase, string exName,
            ExchangeTypes exType, string queueName, TimeSpan? timeout = null)
        {
            builder.AddInMemoryConfig($"{sectionBase}:{RabbitMqExtensions.ConnectionStringField}",
                container.GetConnectionString());
            builder.AddInMemoryConfig($"{sectionBase}:{nameof(SimpleRabbitMqConfiguration.ExName)}", exName);
            builder.AddInMemoryConfig($"{sectionBase}:{nameof(SimpleRabbitMqConfiguration.ExType)}", exType.ToString());
            builder.AddInMemoryConfig($"{sectionBase}:{nameof(SimpleRabbitMqConfiguration.QueueName)}", queueName);

            if (timeout.HasValue)
            {
                var random = EbRandomizer.Create();
                builder.AddInMemoryConfig($"{sectionBase}:{nameof(SimpleRabbitMqConfiguration.ExNameCallback)}",
                    random.String(16));
                builder.AddInMemoryConfig($"{sectionBase}:{nameof(SimpleRabbitMqConfiguration.ExTypeCallback)}",
                    nameof(ExchangeTypes.Fanout));
                builder.AddInMemoryConfig($"{sectionBase}:{nameof(SimpleRabbitMqConfiguration.QueueNameCallback)}",
                    random.String(16));
                builder.AddInMemoryConfig($"{sectionBase}:{nameof(SimpleRabbitMqConfiguration.TimeoutCallback)}",
                    timeout.Value.ToString());
            }
        }
    }
}
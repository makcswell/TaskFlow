using System.Text;
using EBCEYS.RabbitMQ.Configuration;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Ebceys.Infrastructure.Options;

/// <summary>
///     Extension methods for <see cref="IConfiguration" /> to read and construct
///     <see cref="RabbitMQConfiguration" /> from configuration sections.
///     Supports both <see cref="SimpleRabbitMqConfiguration" /> shorthand and full detailed configuration.
/// </summary>
[PublicAPI]
public static class RabbitMqExtensions
{
    internal const string ConnectionStringField = "ConnectionString";
    internal const string ExchangeConfField = "ExchangeConfiguration";
    internal const string QueueConfField = "QueueConfiguration";
    internal const string QoSConfField = "QoSConfiguration";
    internal const string CallbackConfField = "CallbackConfiguration";
    internal const string OnStartConfField = "OnStartConfiguration";
    internal const string CreateChannelConfField = "CreateChannelConfiguration";


    extension(IConfiguration configuration)
    {
        /// <summary>
        ///     Gets the <see cref="RabbitMQConfiguration" /> from <paramref name="section" /> of <paramref name="configuration" />
        ///     .
        /// </summary>
        /// <param name="section">The section name.</param>
        /// <returns>The new instance of <see cref="RabbitMQConfiguration" />.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMQConfiguration GetRabbitMqConfiguration(string section)
        {
            var conf = configuration.GetSection(section);
            if (conf.GetValue<string>(nameof(SimpleRabbitMqConfiguration.ExName)) != null)
            {
                var config = conf.Get<SimpleRabbitMqConfiguration>();
                config = config.Validate();
                return config.ToConfig();
            }

            var connectionString = conf.GetValue<Uri>(ConnectionStringField)
                                   ?? throw new ArgumentNullException(ConnectionStringField);

            var exchangeConf = conf.GetExchangeConfiguration(ExchangeConfField)
                               ?? throw new ArgumentNullException(ExchangeConfField);

            var queueConf = conf.GetQueueConfiguration(QueueConfField)
                            ?? throw new ArgumentNullException(QueueConfField);

            var qosConf = conf.GetQoSConfiguration(QoSConfField);
            var callbackConf = conf.GetCallbackConfiguration(CallbackConfField);
            var onStartConf = conf.GetOnStartConfigs(OnStartConfField);
            var channelConf = conf.GetCreateChannelOptions(CreateChannelConfField);


            var builder = new RabbitMQConfigurationBuilder()
                .AddConnectionFactory(new ConnectionFactory
                {
                    Uri = connectionString
                }).AddEncoding(Encoding.UTF8)
                .AddExchangeConfiguration(exchangeConf)
                .AddQueueConfiguration(queueConf);
            if (qosConf is not null)
            {
                builder.AddQoSConfiguration(qosConf);
            }

            if (callbackConf is not null)
            {
                builder.AddCallbackConfiguration(callbackConf);
            }

            if (onStartConf is not null)
            {
                builder.AddOnStartConfiguration(onStartConf);
            }

            if (channelConf is not null)
            {
                builder.AddCreateChannelOptions(channelConf);
            }

            return builder.Build();
        }

        /// <summary>
        ///     Gets the <see cref="ExchangeConfiguration" /> from <paramref name="configuration" /> by
        ///     <paramref name="fieldName" />.
        /// </summary>
        /// <param name="fieldName">The configuration section name.</param>
        /// <returns>The <see cref="ExchangeConfiguration" /> instance, or <c>null</c> if the section is missing.</returns>
        public ExchangeConfiguration? GetExchangeConfiguration(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<ExchangeConfiguration>();
        }

        /// <summary>
        ///     Gets the <see cref="QueueConfiguration" /> from the specified configuration section.
        /// </summary>
        /// <param name="fieldName">The configuration section name.</param>
        /// <returns>The <see cref="QueueConfiguration" /> instance, or <c>null</c> if the section is missing.</returns>
        public QueueConfiguration? GetQueueConfiguration(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<QueueConfiguration>();
        }

        /// <summary>
        ///     Gets the <see cref="QoSConfiguration" /> from the specified configuration section.
        /// </summary>
        /// <param name="fieldName">The configuration section name.</param>
        /// <returns>The <see cref="QoSConfiguration" /> instance, or <c>null</c> if the section is missing.</returns>
        public QoSConfiguration? GetQoSConfiguration(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<QoSConfiguration>();
        }

        /// <summary>
        ///     Gets the <see cref="CallbackRabbitMQConfiguration" /> from the specified configuration section.
        /// </summary>
        /// <param name="fieldName">The configuration section name.</param>
        /// <returns>The <see cref="CallbackRabbitMQConfiguration" /> instance, or <c>null</c> if the section is missing.</returns>
        public CallbackRabbitMQConfiguration? GetCallbackConfiguration(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<CallbackRabbitMQConfiguration>();
        }

        /// <summary>
        ///     Gets the <see cref="RabbitMQOnStartConfigs" /> from the specified configuration section.
        /// </summary>
        /// <param name="fieldName">The configuration section name.</param>
        /// <returns>The <see cref="RabbitMQOnStartConfigs" /> instance, or <c>null</c> if the section is missing.</returns>
        public RabbitMQOnStartConfigs? GetOnStartConfigs(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<RabbitMQOnStartConfigs>();
        }

        /// <summary>
        ///     Gets the <see cref="CreateChannelOptions" /> from the specified configuration section.
        /// </summary>
        /// <param name="fieldName">The configuration section name.</param>
        /// <returns>The <see cref="CreateChannelOptions" /> instance, or <c>null</c> if the section is missing.</returns>
        public CreateChannelOptions? GetCreateChannelOptions(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<CreateChannelOptions>();
        }
    }
}
using System.Text;
using Ebceys.Infrastructure.RabbitMq;
using Ebceys.Infrastructure.Validation;
using EBCEYS.RabbitMQ.Configuration;
using FluentValidation;
using JetBrains.Annotations;
using RabbitMQ.Client;

namespace Ebceys.Infrastructure.Options;

/// <summary>
///     Simplified RabbitMQ configuration record that can be bound from <c>appsettings.json</c>.
///     Provides a convenient shorthand for configuring exchange, queue, and optional callback settings.
///     Use <see cref="ToConfig" /> to convert to a full <see cref="RabbitMQConfiguration" />.
/// </summary>
[PublicAPI]
public record SimpleRabbitMqConfiguration
{
    /// <summary>
    ///     The connection string.
    /// </summary>
    public required string ConnectionString { get; init; }

    /// <summary>
    ///     The exchange name.
    /// </summary>
    public required string ExName { get; init; }

    /// <summary>
    ///     The exchange type.
    /// </summary>
    public ExchangeTypes ExType { get; init; } = ExchangeTypes.Fanout;

    /// <summary>
    ///     The queue name.
    /// </summary>
    public required string QueueName { get; init; }

    /// <summary>
    ///     The routing key. If null, the <see cref="QueueName" /> will be used.
    /// </summary>
    public string? RoutingKey { get; init; }

    /// <summary>
    ///     The timeout for callback. Sets the availability to use <see cref="EbRabbitMqClient.SendRequestAsync{T}" />.
    /// </summary>
    public TimeSpan? TimeoutCallback { get; init; }

    /// <summary>
    ///     The exchange name for callback.
    /// </summary>
    public string? ExNameCallback { get; init; }

    /// <summary>
    ///     The exchange type for callback.
    /// </summary>
    public ExchangeTypes ExTypeCallback { get; init; } = ExchangeTypes.Fanout;

    /// <summary>
    ///     The queue name for callback.
    /// </summary>
    public string? QueueNameCallback { get; init; }

    /// <summary>
    ///     Creates the new instance of <see cref="RabbitMQConfiguration" /> based on
    ///     <see cref="SimpleRabbitMqConfiguration" />.
    /// </summary>
    /// <returns></returns>
    public RabbitMQConfiguration ToConfig()
    {
        var builder = new RabbitMQConfigurationBuilder()
            .AddConnectionFactory(new ConnectionFactory
            {
                Uri = new Uri(ConnectionString)
            })
            .AddEncoding(Encoding.UTF8)
            .AddExchangeConfiguration(new ExchangeConfiguration(ExName, ExType, autoDelete: true))
            .AddQueueConfiguration(new QueueConfiguration(QueueName, RoutingKey ?? QueueName, autoDelete: true))
            .AddOnStartConfiguration(new RabbitMQOnStartConfigs
            {
                ConnectionReties = 3,
                DelayBeforeRetries = TimeSpan.FromSeconds(3),
                ThrowServerExceptionsOnReceivingResponse = true
            });
        if (TimeoutCallback is not null)
        {
            builder.AddCallbackConfiguration(new CallbackRabbitMQConfiguration(
                new QueueConfiguration(QueueNameCallback!, QueueNameCallback, autoDelete: true),
                TimeoutCallback!.Value, new ExchangeConfiguration(ExNameCallback!, ExTypeCallback, autoDelete: true)));
        }

        return builder.Build();
    }
}

/// <summary>
///     Validation extension methods for <see cref="SimpleRabbitMqConfiguration" />.
/// </summary>
[PublicAPI]
public static class SimpleRabbitMqConfigurationExtensions
{
    private static readonly SimpleRabbitMqConfigurationValidator Validator = new();

    extension(SimpleRabbitMqConfiguration? configuration)
    {
        /// <summary>
        ///     Validates the <paramref name="configuration" />.
        /// </summary>
        /// <exception cref="ValidationException">Throw if <paramref name="configuration" /> is not valid.</exception>
        public SimpleRabbitMqConfiguration Validate()
        {
            if (configuration is null)
            {
                throw new ValidationException($"The {nameof(SimpleRabbitMqConfiguration)} cannot be null!)");
            }

            Validator.ValidateAndThrow(configuration);

            return configuration;
        }
    }
}

internal class SimpleRabbitMqConfigurationValidator : AbstractValidator<SimpleRabbitMqConfiguration>
{
    public SimpleRabbitMqConfigurationValidator()
    {
        RuleFor(x => x).NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.ConnectionString).NotEmpty().IsValidAbsoluteUrl();
                RuleFor(x => x.ExName).NotNull().NotEmpty();
                RuleFor(x => x.ExType).IsInEnum();
                RuleFor(x => x.QueueName).NotNull().NotEmpty();
                RuleFor(x => x.TimeoutCallback).GreaterThan(TimeSpan.Zero).DependentRules(() =>
                {
                    RuleFor(x => x.ExNameCallback).NotNull().NotEmpty();
                    RuleFor(x => x.QueueNameCallback).NotNull().NotEmpty();
                    RuleFor(x => x.ExTypeCallback).IsInEnum();
                }).When(x => x.TimeoutCallback is not null);
            });
    }
}
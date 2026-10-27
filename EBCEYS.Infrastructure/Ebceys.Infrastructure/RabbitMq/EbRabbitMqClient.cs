using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ebceys.Infrastructure.RabbitMq;

/// <summary>
///     Abstract wrapper class around <see cref="RabbitMQClient" /> that provides convenient
///     typed message sending and request/response methods. Derive from this class to create
///     application-specific RabbitMQ clients. Registered as a hosted service for automatic lifecycle management.
/// </summary>
[PublicAPI]
public abstract class EbRabbitMqClient : IRabbitMQClient
{
    private readonly RabbitMQClient _client;

    /// <summary>
    ///     Initiates the new instance of <see cref="EbRabbitMqClient" />.
    /// </summary>
    /// <param name="clientLogger">The client logger.</param>
    /// <param name="config">The rabbitmq configuration.</param>
    /// <param name="serializerSettings">The JSON serializer settings.</param>
    protected EbRabbitMqClient(ILogger<RabbitMQClient> clientLogger, RabbitMQConfiguration config,
        JsonSerializerSettings? serializerSettings = null)
    {
        ConnectionString = config.Factory.Uri;
        _client = new RabbitMQClient(clientLogger, config, serializerSettings);
    }

    internal Uri ConnectionString { get; }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _client.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _client.StopAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }

    /// <inheritdoc />
    public Task SendMessageAsync(RabbitMQRequestData data, bool mandatory = false,
        CancellationToken token = default)
    {
        return _client.SendMessageAsync(data, mandatory, token);
    }


    /// <inheritdoc />
    public Task<T?> SendRequestAsync<T>(RabbitMQRequestData data, bool mandatory = false,
        CancellationToken token = default)
    {
        return _client.SendRequestAsync<T>(data, mandatory, token);
    }

    /// <summary>
    ///     Sends the messages.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="method">The method name.</param>
    /// <param name="mandatory">The mandatory.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public Task SendMessageAsync<TMessage>(TMessage message, string method, bool mandatory = false,
        CancellationToken token = default) where TMessage : notnull
    {
        return _client.SendMessageAsync(new RabbitMQRequestData
        {
            Method = method,
            Params = [message]
        }, mandatory, token);
    }

    /// <summary>
    ///     Sends the messages.
    /// </summary>
    /// <param name="method">The method name.</param>
    /// <param name="mandatory">The mandatory.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public Task SendMessageAsync(string method, bool mandatory = false,
        CancellationToken token = default)
    {
        return _client.SendMessageAsync(new RabbitMQRequestData
        {
            Method = method
        }, mandatory, token);
    }

    /// <summary>
    ///     Sends the request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="method">The method name.</param>
    /// <param name="mandatory">The mandatory.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public Task<TResponse?> SendRequestAsync<TRequest, TResponse>(TRequest request, string method,
        bool mandatory = false,
        CancellationToken token = default)
        where TRequest : notnull
    {
        return _client.SendRequestAsync<TResponse>(new RabbitMQRequestData
        {
            Method = method,
            Params = [request]
        }, mandatory, token);
    }

    /// <summary>
    ///     Sends the request.
    /// </summary>
    /// <param name="method">The method name.</param>
    /// <param name="mandatory">The mandatory.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public Task<TResponse?> SendRequestAsync<TResponse>(string method, bool mandatory = false,
        CancellationToken token = default)
    {
        return _client.SendRequestAsync<TResponse>(new RabbitMQRequestData
        {
            Method = method
        }, mandatory, token);
    }
}
using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.RabbitMq;
using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Ebceys.Infrastructure.AuthorizationTestApplication.Client;

/// <summary>
///     The <see cref="AppRabbitClient" /> class.
/// </summary>
[PublicAPI]
public class AppRabbitClient(
    ILogger<RabbitMQClient> clientLogger,
    RabbitMQConfiguration config,
    JsonSerializerSettings? serializerSettings = null)
    : EbRabbitMqClient(clientLogger, config, serializerSettings), IAppRabbitClient
{
    /// <inheritdoc />
    public Task<string?> GetOkAsync(CancellationToken token = default)
    {
        return SendRequestAsync<string?>(RoutesDictionary.RabbitMqControllerV1.Methods.GetOk, token: token);
    }

    /// <inheritdoc />
    public Task<GenerateTokenResponse?> GenerateTokenAsync(GenerateTokenRequest request,
        CancellationToken token = default)
    {
        return SendRequestAsync<GenerateTokenRequest, GenerateTokenResponse>(request,
            RoutesDictionary.RabbitMqControllerV1.Methods.GetJson, token: token);
    }
}
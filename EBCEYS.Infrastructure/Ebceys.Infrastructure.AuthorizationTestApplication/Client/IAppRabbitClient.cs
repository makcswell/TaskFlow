using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;

namespace Ebceys.Infrastructure.AuthorizationTestApplication.Client;

public interface IAppRabbitClient
{
    Task<string?> GetOkAsync(CancellationToken token = default);
    Task<GenerateTokenResponse?> GenerateTokenAsync(GenerateTokenRequest request, CancellationToken token = default);
}
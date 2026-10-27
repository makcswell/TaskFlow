using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Responses;

namespace Ebceys.Infrastructure.TestApplication.Client.Interfaces;

public interface ITestClient
{
    Task GetOkAsync(CancellationToken token);
    Task<SomeBodyResponse> GetJsonAsync(CancellationToken token);
    Task GetExceptionAsync(CancellationToken token);
    Task<SomeBodyResponse> PostBodyAsync(SomeBodyRequest body, CancellationToken token);
    Task<SomeBodyResponse> GetQueryAsync(int value, CancellationToken token);
    Task<CommandResultResponse> PostCommandAsync(string name, CancellationToken token);
    Task<CommandResultResponse> GetCommandAsync(CancellationToken token);
    Task DeleteCommandAsync(string name, CancellationToken token);
    Task PutCommandAsync(string name, ChangeNameRequest request, CancellationToken token);

    Task<GenerateTokenResponse> GenerateTokenAsync(CancellationToken token);
    Task ValidateTokenAsync(string jwt, CancellationToken token);
    Task ValidateAuthAsync(string jwt, CancellationToken token);

    Task NonExistsMethodAsync(CancellationToken token);

    Task<CommandResultResponse> MethodWithoutResponseBodyAsync(CancellationToken token);
}
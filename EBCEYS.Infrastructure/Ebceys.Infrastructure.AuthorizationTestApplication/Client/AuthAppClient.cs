using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.HttpClient;
using Ebceys.Infrastructure.HttpClient.Extensions;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Mvc;
using static Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext.RoutesDictionary.AuthControllerV1;

namespace Ebceys.Infrastructure.AuthorizationTestApplication.Client;

public interface IAuthAppClient
{
    Task<GenerateTokenResponse> GenerateTokenAsync(GenerateTokenRequest request,
        CancellationToken token = default);

    Task ValidateTokenAsync(CancellationToken token = default);

    Task ValidateAuthAsync(CancellationToken token = default);
}

public class AuthAppClient(
    IFlurlClientCache clientCache,
    ILoggerFactory loggerFactory,
    ClientBaseUrlResolver baseUrlResolver,
    ClientBaseTokenResolver? tokenResolver = null)
    : ClientBase(clientCache, loggerFactory, baseUrlResolver, tokenResolver), IAuthAppClient
{
    public async Task<GenerateTokenResponse> GenerateTokenAsync(GenerateTokenRequest request,
        CancellationToken token = default)
    {
        var response =
            await this.PostJsonAsync<GenerateTokenRequest, GenerateTokenResponse, ProblemDetails>(
                url => url.AppendPathSegments(
                    BaseRoute, Methods.GetToken),
                request,
                null,
                token);

        return response.GetResponseOrThrow();
    }

    public async Task ValidateTokenAsync(CancellationToken token = default)
    {
        var response = await this.PostAsync<ProblemDetails>(
            url => url.AppendPathSegments(BaseRoute, Methods.ValidateToken),
            token: token);

        response.ThrowIfUnsuccess();
    }

    public async Task ValidateAuthAsync(CancellationToken token = default)
    {
        var response = await this.GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BaseRoute, Methods.ValidateAuth), token: token);

        response.ThrowIfUnsuccess();
    }
}
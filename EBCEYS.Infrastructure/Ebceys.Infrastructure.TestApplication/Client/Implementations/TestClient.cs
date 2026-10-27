using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.HttpClient;
using Ebceys.Infrastructure.HttpClient.Extensions;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Responses;
using Ebceys.Infrastructure.TestApplication.Client.Interfaces;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Mvc;
using RoutesDictionary = Ebceys.Infrastructure.TestApplication.BoundedContext.RoutesDictionary;

namespace Ebceys.Infrastructure.TestApplication.Client.Implementations;

public class TestClient(
    IFlurlClientCache clientCache,
    ILoggerFactory loggerFactory,
    ClientBaseUrlResolver baseUrlResolver,
    ClientBaseTokenResolver? tokenResolver = null)
    : ClientBase(clientCache, loggerFactory, baseUrlResolver, tokenResolver), ITestClient
{
    private const string BasePath = RoutesDictionary.TestControllerV1.BaseRoute;

    public async Task GetOkAsync(CancellationToken token)
    {
        var result = await this.GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetOk), token: token);

        result.ThrowIfUnsuccess();
    }

    public async Task<SomeBodyResponse> GetJsonAsync(CancellationToken token)
    {
        var result = await this.GetJsonAsync<SomeBodyResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetJson),
            token: token);

        return result.GetResponseOrThrow();
    }

    public async Task GetExceptionAsync(CancellationToken token)
    {
        var result = await this.GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetException),
            token: token);

        result.ThrowIfUnsuccess();
    }

    public async Task<SomeBodyResponse> PostBodyAsync(SomeBodyRequest body, CancellationToken token)
    {
        var result = await this.PostJsonAsync<SomeBodyRequest, SomeBodyResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.PostBody),
            body,
            token: token);
        return result.GetResponseOrThrow();
    }

    public async Task<SomeBodyResponse> GetQueryAsync(int value, CancellationToken token)
    {
        var result = await this.GetJsonAsync<SomeBodyResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetQuery)
                .AppendQueryParam("value", value),
            token: token);

        return result.GetResponseOrThrow();
    }

    public async Task<CommandResultResponse> PostCommandAsync(string name, CancellationToken token)
    {
        var result = await this.PostJsonAsync<CommandResultResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.PostCommand)
                .AppendQueryParam("name", name),
            token: token);

        return result.GetResponseOrThrow();
    }

    public async Task<CommandResultResponse> GetCommandAsync(CancellationToken token)
    {
        var result = await this.GetJsonAsync<CommandResultResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetCommand),
            token: token);

        return result.GetResponseOrThrow();
    }

    public async Task DeleteCommandAsync(string name, CancellationToken token)
    {
        var result = await this.DeleteAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.DeleteCommand)
                .AppendQueryParam("name", name),
            token: token);

        result.ThrowIfUnsuccess();
    }

    public async Task PutCommandAsync(string name, ChangeNameRequest request, CancellationToken token)
    {
        var result = await this.PutJsonAsync<ChangeNameRequest, ProblemDetails>(url => url
                .AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.PutCommand)
                .AppendQueryParam("name", name),
            request,
            token: token);

        result.ThrowIfUnsuccess();
    }

    public async Task<GenerateTokenResponse> GenerateTokenAsync(CancellationToken token)
    {
        var result = await this.GetJsonAsync<GenerateTokenResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetToken),
            token: token);

        if (!result.IsSuccess || result.Result is null)
        {
            return ApiExceptionHelper.ThrowApiException(result);
        }

        return result.Result;
    }

    public async Task ValidateTokenAsync(string jwt, CancellationToken token)
    {
        var result = await this.GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.ValidateToken),
            GetAuthHeaders(jwt), token);

        result.ThrowIfUnsuccess();
    }

    public async Task ValidateAuthAsync(string jwt, CancellationToken token)
    {
        var result = await this.GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.ValidateAuth),
            GetAuthHeaders(jwt),
            token);

        result.ThrowIfUnsuccess();
    }

    public async Task NonExistsMethodAsync(CancellationToken token)
    {
        var result = await this.GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, "some", "non", "exists", "method"),
            null,
            token);

        if (!result.IsSuccess)
        {
            ApiExceptionHelper.ThrowApiException(result.Error);
        }
    }

    public async Task<CommandResultResponse> MethodWithoutResponseBodyAsync(CancellationToken token)
    {
        var jwt = await GenerateTokenAsync(token);

        var result = await this.GetJsonAsync<CommandResultResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.ValidateToken),
            GetAuthHeaders(jwt.Token), token);

        return result.GetResponseOrThrow();
    }
}
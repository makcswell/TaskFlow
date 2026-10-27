using System.Security.Claims;
using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.Helpers.Jwt;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.AuthorizationTestApplication.Rabbit;

using Routes = RoutesDictionary.RabbitMqControllerV1.Methods;

[UsedImplicitly]
[PublicAPI]
public class RabbitMqController(IJwtGenerator jwtGenerator) : RabbitMQSmartControllerBase
{
    [RabbitMQMethod(Routes.GetOk)]
    public Task<string> GetOk()
    {
        return Task.FromResult("Ok");
    }

    [RabbitMQMethod(Routes.GetJson)]
    public Task<GenerateTokenResponse> GenerateToken(GenerateTokenRequest request)
    {
        var token = jwtGenerator.GenerateKey(request.UserName.ToClaim(ClaimTypes.NameIdentifier));
        return Task.FromResult(new GenerateTokenResponse(token));
    }
}
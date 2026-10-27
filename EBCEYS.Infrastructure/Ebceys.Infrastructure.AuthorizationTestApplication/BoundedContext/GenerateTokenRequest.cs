using System.Security.Claims;

namespace Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;

public record GenerateTokenRequest(string UserName)
{
    public const string UserNameClaimType = ClaimTypes.NameIdentifier;
}

public record GenerateTokenResponse(string Token);
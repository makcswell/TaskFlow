using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Helpers.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.AuthorizationTestApplication.Controllers;

[ApiController]
[ApiVersion(RoutesDictionary.AuthControllerV1.ApiVersion)]
[Route(RoutesDictionary.AuthControllerV1.ControllerRoute)]
public class AuthController : ControllerBase
{
    private const string AuthHeader = "Authorization";

    [HttpPost(RoutesDictionary.AuthControllerV1.Methods.GetToken)]
    public IActionResult GetToken(
        [FromBody] [Required] GenerateTokenRequest request,
        [FromServices] IJwtGenerator jwtGenerator)
    {
        var token = jwtGenerator.GenerateKey(request.UserName.ToClaim(GenerateTokenRequest.UserNameClaimType));
        return Ok(new GenerateTokenResponse(token));
    }

    [HttpPost(RoutesDictionary.AuthControllerV1.Methods.ValidateToken)]
    [AllowAnonymous]
    public IActionResult ValidateToken()
    {
        if (!Request.Headers.TryGetValue(AuthHeader, out var value))
        {
            return ApiExceptionHelper.ThrowApiException<IActionResult>(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Detail = "Not Authorized",
                Title = "Unauthorized"
            });
        }

        var token = new JwtSecurityTokenHandler().ReadJwtToken(value.ToString().Replace("Bearer ", ""));
        var usernameClaim = token?.Claims.FirstOrDefault(c => c.Type == GenerateTokenRequest.UserNameClaimType);
        if (usernameClaim is null)
        {
            return ApiExceptionHelper.ThrowApiException<IActionResult>(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Detail = "Not Authorized",
                Title = "Unauthorized"
            });
        }

        return Ok();
    }

    [HttpGet(RoutesDictionary.AuthControllerV1.Methods.ValidateAuth)]
    [Authorize]
    public IActionResult ValidateAuth()
    {
        return Ok();
    }
}
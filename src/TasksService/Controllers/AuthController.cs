using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Helpers.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Data;
using TasksService.Routes;

namespace TasksService.Controllers;

/// <summary>
/// Контроллер аутентификации.
/// </summary>
[ApiController]
[Route(RoutesDictionary.Auth.Controller)]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Получить токен.
    /// </summary>
    [HttpPost(RoutesDictionary.Auth.ValidateAuth)]
    public IActionResult GetToken(
        [FromBody] [Required] GenerateTokenRequest request,
        [FromServices] IJwtGenerator jwtGenerator)
    {
        var token = jwtGenerator.GenerateKey(request.UserName.ToClaim(ClaimTypes.NameIdentifier));
        return Ok(new GenerateTokenResponse(token));
    }

    /// <summary>
    /// Ручная валидация токена.
    /// </summary>
    [HttpPost(RoutesDictionary.Auth.ValidateToken)]
    [AllowAnonymous]
    public IActionResult ValidateToken()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var value))
        {
            return ApiExceptionHelper.ThrowApiException<IActionResult>(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Detail = "Not Authorized",
                Title = "Unauthorized"
            });
        }

        var token = new JwtSecurityTokenHandler().ReadJwtToken(value.ToString().Replace("Bearer ", ""));
        var usernameClaim = token?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
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

    /// <summary>
    /// GetById Task model
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult GetIdGuid([FromQuery] Guid id)
    {
        return Ok();
    }

    /// <summary>
    /// Проверка валидности токена через атрибут Authorize.
    /// </summary>
    [HttpGet(RoutesDictionary.Auth.ValidateAuth)]
    [Authorize]
    public IActionResult ValidateAuth()
    {
        return Ok();
    }
}
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Helpers.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasksService.Routes;

namespace TasksService.Controllers;

/// <summary>
/// Запрос на генерацию токена.
/// </summary>
/// <param name="UserName">Имя пользователя.</param>
public record GenerateTokenRequest(string UserName);

/// <summary>
/// Ответ с токеном.
/// </summary>
/// <param name="Token">JWT-токен.</param>
public record GenerateTokenResponse(string Token);

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
    [HttpPost("token")]
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
    [HttpPost("validate-token")]
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
    /// Проверка валидности токена через атрибут Authorize.
    /// </summary>
    [HttpGet("validate-auth")]
    [Authorize]
    public IActionResult ValidateAuth()
    {
        return Ok();
    }
}
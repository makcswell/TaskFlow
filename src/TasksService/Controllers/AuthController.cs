using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Ebceys.Infrastructure.Helpers;
using Ebceys.Infrastructure.Helpers.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Data;
using TasksService.Routes;

namespace TasksService.Controllers;

/// <summary>
/// Auth controller.
/// </summary>
[ApiController]
[Route(RoutesDictionary.Auth.Controller)]
public class AuthController : ControllerBase
{
    private readonly ICommandExecutor _executor;

    /// <summary>
    /// Command executor injection
    /// </summary>
    /// <param name="executor"></param>
    public AuthController(ICommandExecutor executor)
    {
        _executor = executor;
    }

    /// <summary>
    /// Get auth token.
    /// </summary>
    /// <param name="request">The auth request model.</param>
    /// <param name="jwtGenerator">JWT generator service.</param>
    /// <returns>The auth token.</returns>
    [HttpPost(RoutesDictionary.Auth.GetToken)] // <-- Исправил на GetToken
    [ProducesResponseType(typeof(GenerateTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetToken(
        [FromBody] [Required] GenerateTokenRequest request,
        [FromServices] IJwtGenerator jwtGenerator)
    {
        var token = jwtGenerator.GenerateKey(request.UserName.ToClaim(ClaimTypes.NameIdentifier));
        return Ok(new GenerateTokenResponse(token));
    }

    /// <summary>
    /// Manual token validation.
    /// </summary>
    [HttpPost(RoutesDictionary.Auth.ValidateToken)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken()
    {
        await _executor.ExecuteCommandAsync<IHeaderDictionary, bool>(Request.Headers);
        return Ok();
    }


    /// <summary>
    /// Get task by ID.
    /// </summary>
    /// <param name="id">Task ID.</param>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetIdGuid([FromQuery] Guid id)
    {
        return Ok();
    }

    /// <summary>
    /// Validate token via Authorize attribute.
    /// </summary>
    [HttpGet(RoutesDictionary.Auth.ValidateAuth)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ValidateAuth()
    {
        return Ok();
    }
}
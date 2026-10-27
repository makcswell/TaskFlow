using System.Security.Claims;
using Ebceys.Infrastructure.Exceptions; 
using Ebceys.Infrastructure.Jwt;      
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TasksService.Controllers;


public record GenerateTokenRequest(string UserName);
public record GenerateTokenResponse(string Token);

    [ApiController]
    [ApiVersion(RoutesDictionary.AuthControllerV1.ApiVersion)]
    [Route("auth")]
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
   
    [HttpPost("token")]
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

    [HttpGet("validate-auth")]
    [Authorize]
    public IActionResult ValidateAuth()
    {
        return Ok();
    }
}
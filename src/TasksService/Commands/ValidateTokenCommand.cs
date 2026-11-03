using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Interfaces; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TasksService.Commands;

/// <summary>
/// Command to validate JWT token from header.
/// </summary>
public class ValidateTokenCommand : ICommand<IHeaderDictionary, bool>
{
    /// <summary>
    /// Execute command.
    /// </summary>
    /// <param name="context">HTTP Request headers.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>True if valid.</returns>
    public Task<bool> ExecuteAsync(IHeaderDictionary context, CancellationToken token = default)
    {
        
        if (!context.TryGetValue("Authorization", out var value))
        {
            ApiExceptionHelper.ThrowApiException<bool>(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Detail = "Not Authorized",
                Title = "Unauthorized"
            });
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(value.ToString().Replace("Bearer ", ""));
        var usernameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        
        if (usernameClaim is null)
        {
            ApiExceptionHelper.ThrowApiException<bool>(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Detail = "Not Authorized",
                Title = "Unauthorized"
            });
        }

        return Task.FromResult(true);
    }
}
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Ebceys.Infrastructure.Helpers.Jwt;

/// <summary>
///     Interface for validating and parsing JWT tokens.
/// </summary>
[PublicAPI]
public interface IJwtValidator
{
    /// <summary>
    ///     Tries to validate and parse the specified JWT <paramref name="token" />.
    /// </summary>
    /// <param name="token">The JWT token string (may include the "Bearer " prefix).</param>
    /// <param name="jwtSecurityToken">
    ///     The parsed <see cref="JwtSecurityToken" /> if validation succeeds; otherwise <c>null</c>
    ///     .
    /// </param>
    /// <returns><c>true</c> if the token was successfully parsed; otherwise <c>false</c>.</returns>
    bool TryValidate(string token, [NotNullWhen(true)] out JwtSecurityToken? jwtSecurityToken);
}

/// <summary>
///     Default implementation of <see cref="IJwtValidator" /> that parses JWT tokens
///     by stripping the "Bearer " prefix and reading the token using <see cref="JwtSecurityTokenHandler" />.
/// </summary>
/// <param name="logger">The logger for validation warnings.</param>
[PublicAPI]
public class JwtValidator(ILogger<JwtValidator> logger) : IJwtValidator
{
    /// <inheritdoc />
    public bool TryValidate(string token, [NotNullWhen(true)] out JwtSecurityToken? jwtSecurityToken)
    {
        try
        {
            var jwt = token.Replace($"{JwtGenerator.AuthSchema} ", "");
            jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error on validating jwt");
            jwtSecurityToken = null;
            return false;
        }
    }
}
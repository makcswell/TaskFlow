using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ebceys.Infrastructure.Options;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ebceys.Infrastructure.Helpers.Jwt;

/// <summary>
///     Interface for generating JWT tokens with specified claims.
/// </summary>
[PublicAPI]
public interface IJwtGenerator
{
    /// <summary>
    ///     Generates the JWT with specified <paramref name="claims" />.
    /// </summary>
    /// <param name="claims">The claims.</param>
    /// <returns>The new jwt token.</returns>
    string GenerateKey(params IEnumerable<Claim> claims);
}

/// <summary>
///     Internal JWT generator that creates signed JWT tokens using HMAC-SHA256 with configurable
///     issuer, audience, and optional token expiration from <see cref="JwtOptions" />.
/// </summary>
/// <param name="options">The JWT configuration options.</param>
internal class JwtGenerator(IOptions<JwtOptions> options) : IJwtGenerator
{
    /// <summary>
    ///     The auth schema.
    /// </summary>
    public const string AuthSchema = JwtBearerDefaults.AuthenticationScheme;

    /// <inheritdoc />
    public string GenerateKey(params IEnumerable<Claim> claims)
    {
        DateTime? expires = null;
        if (options.Value.TokenTimeToLive.HasValue && options.Value.TokenTimeToLive.Value > TimeSpan.Zero)
        {
            expires = DateTime.UtcNow.Add(options.Value.TokenTimeToLive.Value);
        }

        var key = new SymmetricSecurityKey(Convert.FromBase64String(options.Value.Base64Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken
        (
            options.Value.Issuer,
            options.Value.Audience,
            claims,
            null,
            expires,
            credentials
        );
        return $"{AuthSchema} {new JwtSecurityTokenHandler().WriteToken(jwt)}";
    }
}
using System.Security.Claims;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.Helpers.Jwt;

/// <summary>
///     Extension methods for convenient JWT-related object conversions.
/// </summary>
[PublicAPI]
public static class JwtObjectsExtensions
{
    /// <summary>
    ///     Gets the <see cref="Claim" /> by <paramref name="value" /> with specified <paramref name="type" />.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type.</param>
    /// <returns>The new instance of <see cref="Claim" />.</returns>
    public static Claim ToClaim(this string value, string type)
    {
        return new Claim(type, value);
    }
}
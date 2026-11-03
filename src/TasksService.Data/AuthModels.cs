using JetBrains.Annotations;

namespace TaskFlow.Data;

/// <summary>
/// Token generation request.
/// </summary>
/// <param name="UserName">User name.</param>
[PublicAPI]
public record UserNameRequest
{
    /// <summary>
    /// User name.
    /// </summary>
    public string UserName { get; init; } = string.Empty;
}

/// <summary>
/// Token generation response.
/// </summary>
/// <param name="Token">JWT token.</param>
[PublicAPI]
public record TokenRequest
{
    /// <summary>
    /// Token.
    /// </summary>
    public string Token { get; init; } = string.Empty;
}
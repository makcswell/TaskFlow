namespace TaskFlow.Data; 

/// <summary>
/// Token generation request.
/// </summary>
/// <param name="UserName">User name.</param>
public record GenerateTokenRequest(string UserName);

/// <summary>
/// Token generation response.
/// </summary>
/// <param name="Token">JWT token.</param>
public record GenerateTokenResponse(string Token);
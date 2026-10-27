using JetBrains.Annotations;

namespace Ebceys.Infrastructure.Attributes;

/// <summary>
///     Attribute that suppresses response body logging in <see cref="Middlewares.RequestLoggingMiddleware" />.
/// </summary>
/// <remarks>
///     Apply this attribute to controller action methods whose response body should not appear in logs
///     (e.g., endpoints returning sensitive data such as tokens or personal information).
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method)]
public class NoResponseBodyLoggingAttribute : Attribute
{
}
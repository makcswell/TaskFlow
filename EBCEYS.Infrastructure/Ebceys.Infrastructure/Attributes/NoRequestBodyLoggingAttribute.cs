using JetBrains.Annotations;

namespace Ebceys.Infrastructure.Attributes;

/// <summary>
///     Attribute that suppresses request body logging in <see cref="Middlewares.RequestLoggingMiddleware" />.
/// </summary>
/// <remarks>
///     Apply this attribute to controller action methods whose request body should not appear in logs
///     (e.g., endpoints receiving sensitive data such as passwords or tokens).
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method)]
public class NoRequestBodyLoggingAttribute : Attribute
{
}
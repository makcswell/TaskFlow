using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Ebceys.Infrastructure.Models;

/// <summary>
///     Immutable record containing service metadata used for Swagger documentation, routing base path,
///     and service identification throughout the infrastructure.
/// </summary>
/// <param name="ServiceName">The display name of the service.</param>
/// <param name="BaseAddress">The base path for the service API (e.g., <c>/api/my-service</c>).</param>
/// <param name="Description">The human-readable description of the service.</param>
[PublicAPI]
public sealed record ServiceApiInfo(
    string ServiceName,
    PathString BaseAddress,
    string Description);
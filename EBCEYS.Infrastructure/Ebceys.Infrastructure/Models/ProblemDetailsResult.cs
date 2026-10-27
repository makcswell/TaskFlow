using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.Models;

/// <summary>
///     An <see cref="ObjectResult" /> that wraps <see cref="ProblemDetails" /> and sets the correct
///     HTTP status code and <c>application/problem+json</c> content type in the response.
/// </summary>
[PublicAPI]
public class ProblemDetailsResult : ObjectResult
{
    /// <summary>
    ///     Initiates the new instance of <see cref="ProblemDetailsResult" />.
    /// </summary>
    /// <param name="value">The problem details containing error information and HTTP status code.</param>
    public ProblemDetailsResult(ProblemDetails value) : base(value)
    {
        StatusCode = value.Status;
        ContentTypes = ["application/problem+json"];
    }
}
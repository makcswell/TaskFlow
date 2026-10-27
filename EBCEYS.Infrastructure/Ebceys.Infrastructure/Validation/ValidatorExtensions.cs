using FluentValidation;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.Validation;

/// <summary>
///     FluentValidation custom rule builder extensions for URL validation.
/// </summary>
[PublicAPI]
public static class ValidatorExtensions
{
    /// <summary>
    ///     Sets the rule that param must be a valid absolute url.
    /// </summary>
    /// <param name="ruleBuilder"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> IsValidAbsoluteUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid URL. Please ensure it is a valid, absolute URL.");
    }

    /// <summary>
    ///     Sets the rule that param must be a valid relative url.
    /// </summary>
    /// <param name="ruleBuilder"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> IsValidRelativeUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(url => Uri.TryCreate(url, UriKind.Relative, out _))
            .WithMessage("Invalid URL. Please ensure it is a valid, Relative URL.");
    }
}
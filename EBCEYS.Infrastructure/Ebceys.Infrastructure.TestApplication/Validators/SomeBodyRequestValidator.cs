using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using FluentValidation;

namespace Ebceys.Infrastructure.TestApplication.Validators;

public class SomeBodyRequestValidator : AbstractValidator<SomeBodyRequest>
{
    public SomeBodyRequestValidator()
    {
        RuleFor(it => it).NotNull()
            .DependentRules(() => { RuleFor(it => it.Message).NotEmpty().MaximumLength(10); });
    }
}
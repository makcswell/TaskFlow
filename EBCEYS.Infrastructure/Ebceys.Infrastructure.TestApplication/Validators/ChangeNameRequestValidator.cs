using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using FluentValidation;

namespace Ebceys.Infrastructure.TestApplication.Validators;

public class ChangeNameRequestValidator : AbstractValidator<ChangeNameRequest>
{
    public ChangeNameRequestValidator()
    {
        RuleFor(it => it).NotNull()
            .DependentRules(() => { RuleFor(it => it.NewName).NotEmpty().MaximumLength(50); });
    }
}
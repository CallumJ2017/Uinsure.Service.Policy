using Application.Models.Request;
using FluentValidation;

namespace Api.Validators;

public class RenewPolicyRequestDtoValidator : AbstractValidator<RenewPolicyRequestDto>
{
    public RenewPolicyRequestDtoValidator()
    {
        RuleFor(x => x.RenewalDate)
            .NotEqual(default(DateOnly))
            .WithMessage("Renewal date is required.");

        RuleFor(x => x.Payment!)
            .SetValidator(new PaymentDtoValidator())
            .When(x => x.Payment is not null);
    }
}

using Application.Models.Request;
using FluentValidation;

namespace Api.Validators;

public class CancelPolicyRequestDtoValidator : AbstractValidator<CancelPolicyRequestDto>
{
    public CancelPolicyRequestDtoValidator()
    {
        RuleFor(x => x.CancellationDate)
            .NotEqual(default(DateOnly))
            .WithMessage("Cancellation date is required.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required.");
    }
}

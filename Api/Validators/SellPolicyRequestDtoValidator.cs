using Application.Models.Request;
using FluentValidation;

namespace Api.Validators;

public class SellPolicyRequestDtoValidator : AbstractValidator<SellPolicyRequestDto>
{
    public SellPolicyRequestDtoValidator()
    {
        // Insurance type is required
        RuleFor(x => x.InsuranceType)
            .NotEmpty().WithMessage("Insurance type is required.");

        // Start date cannot be in the past
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Start date cannot be in the past.");

        // Policyholders must exist and each must be valid
        RuleFor(x => x.Policyholders)
            .NotEmpty().WithMessage("At least one policyholder is required.")
            .ForEach(ph => ph.SetValidator(new PolicyholderDtoValidator()));

        // Property must be provided and valid
        RuleFor(x => x.Property)
            .NotNull().WithMessage("Property information is required.")
            .SetValidator(new PropertyDtoValidator());
    }
}

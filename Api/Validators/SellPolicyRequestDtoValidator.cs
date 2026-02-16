using Application.Models.Request;
using Domain.Enums;
using FluentValidation;

namespace Api.Validators;

public class SellPolicyRequestDtoValidator : AbstractValidator<SellPolicyRequestDto>
{
    public SellPolicyRequestDtoValidator()
    {
        RuleFor(x => x.InsuranceType)
            .NotEmpty().WithMessage("Insurance type is required.")
            .Must(v => Enum.TryParse<HomeInsuranceType>(v, ignoreCase: true, out _))
            .WithMessage("Insurance type is invalid.");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Start date cannot be in the past.");

        RuleFor(x => x.Policyholders)
            .NotEmpty().WithMessage("At least one policyholder is required.")
            .ForEach(ph => ph.SetValidator(new PolicyholderDtoValidator()));

        RuleFor(x => x.Property)
            .NotNull().WithMessage("Property information is required.")
            .SetValidator(new PropertyDtoValidator());
    }
}

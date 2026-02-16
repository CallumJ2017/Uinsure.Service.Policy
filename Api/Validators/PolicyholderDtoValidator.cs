using Application.Dtos;
using FluentValidation;

namespace Api.Validators;


public class PolicyholderDtoValidator : AbstractValidator<PolicyholderDto>
{
    public PolicyholderDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Policyholder first name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Policyholder last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past.");
    }
}
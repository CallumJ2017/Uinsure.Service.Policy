using Application.Dtos;
using FluentValidation;

namespace Api.Validators;

public class PropertyDtoValidator : AbstractValidator<PropertyDto>
{
    public PropertyDtoValidator()
    {
        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required.");

        RuleFor(x => x.Postcode)
            .NotEmpty().WithMessage("Postcode is required.");
    }
}
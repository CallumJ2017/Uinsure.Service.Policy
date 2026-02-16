using Api.Validators;
using FluentValidation;

namespace Api.Extensions;

public static class FluentValidation
{
    public static void AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SellPolicyRequestDtoValidator>();
    }
}
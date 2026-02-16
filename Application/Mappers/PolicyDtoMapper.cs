using Application.Dtos;
using Application.Dtos.Response;
using Domain.Aggregates;

namespace Application.Mappers;

public static class PolicyDtoMapper
{
    public static PolicyDto ToDto(this Policy policy)
    {
        return new PolicyDto
        {
            Id = policy.Id,
            Reference = policy.Reference.Value,
            InsuranceType = policy.InsuranceType.ToString(),
            Status = policy.Status.ToString(),
            StartDate = policy.StartDate,
            EndDate = policy.EndDate,
            Amount = policy.Premium.Value,
            AutoRenew = policy.AutoRenew,
            HasClaims = policy.HasClaims,
            CreatedAt = policy.CreatedAt,
            LastModifiedAt = policy.LastModifiedAt,
            Property = policy.InsuredProperty is null
                ? null
                : new PropertyDto
                {
                    AddressLine1 = policy.InsuredProperty.AddressLine1,
                    AddressLine2 = policy.InsuredProperty.AddressLine2,
                    AddressLine3 = policy.InsuredProperty.AddressLine3,
                    Postcode = policy.InsuredProperty.Postcode
                },
            Policyholders = [.. policy.PolicyHolders.Select(holder => new PolicyholderDto
            {
                FirstName = holder.FirstName,
                LastName = holder.LastName,
                DateOfBirth = holder.DateOfBirth
            })],
            Payments = [.. policy.Payments.Select(payment => new PaymentDto
            {
                Reference = payment.Reference,
                PaymentMethod = payment.Type.ToString(),
                Amount = payment.Amount
            })]
        };
    }
}

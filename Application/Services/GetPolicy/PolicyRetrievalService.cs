using Application.Dtos;
using Application.Dtos.Response;
using Domain.Repository;
using Domain.ValueObjects;
using SharedKernel;

namespace Application.Services.GetPolicy;

public class PolicyRetrievalService : IPolicyRetrievalService
{
    private readonly IPolicyRepository _policyRepository;

    public PolicyRetrievalService(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<Result<PolicyDto>> GetPolicyAsync(string policyReference)
    {
        var policy = await _policyRepository.GetByReferenceAsync(PolicyReference.FromString(policyReference));

        if (policy is null)
            return Result<PolicyDto>.Fail("policy.not_found", $"Policy with reference {policyReference} does not exist.");

        var dto = new PolicyDto
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

        return Result<PolicyDto>.Success(dto);
    }
}

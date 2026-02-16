using Application.Dtos;
using Application.Dtos.Response;
using Application.Models.Request;
using Domain.Enums;
using Domain.Repository;
using Domain.ValueObjects;
using SharedKernel;

namespace Application.Services.RenewPolicy;

public class PolicyRenewalService : IPolicyRenewalService
{
    private readonly IPolicyRepository _policyRepository;

    public PolicyRenewalService(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<Result<RenewPolicyResponseDto>> RenewPolicyAsync(string policyReference, RenewPolicyRequestDto request)
    {
        PaymentMethod? paymentMethod = null;

        if (request.Payment is not null)
        {
            if (!Enum.TryParse<PaymentMethod>(request.Payment.PaymentMethod, ignoreCase: true, out var parsedPaymentMethod))
                return Result<RenewPolicyResponseDto>.Fail("payment.invalid_type", "Payment type is invalid.");

            paymentMethod = parsedPaymentMethod;
        }

        var policy = await _policyRepository.GetByReferenceAsync(PolicyReference.FromString(policyReference));
        if (policy is null)
            return Result<RenewPolicyResponseDto>.Fail("policy.not_found", $"Policy with reference {policyReference} does not exist.");

        var renewResult = policy.Renew(
            request.RenewalDate,
            request.Payment?.Reference,
            paymentMethod,
            request.Payment?.Amount);

        if (!renewResult.IsSuccess)
            return Result<RenewPolicyResponseDto>.Fail(renewResult.Error!.Code, renewResult.Error.Message);

        await _policyRepository.SaveChangesAsync();

        var policyDto = new PolicyDto
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

        return Result<RenewPolicyResponseDto>.Success(new RenewPolicyResponseDto(policyDto));
    }
}

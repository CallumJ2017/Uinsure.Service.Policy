using Application.Dtos.Response;
using Application.Mappers;
using Application.Models.Command;
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

    public async Task<Result<RenewPolicyResponseDto>> RenewPolicyAsync(string policyReference, RenewPolicyCommand command)
    {
        var policy = await _policyRepository.GetByReferenceAsync(PolicyReference.FromString(policyReference));
        if (policy is null)
            return Result<RenewPolicyResponseDto>.Fail("policy.not_found", $"Policy with reference {policyReference} does not exist.");

        var renewResult = policy.Renew(
            command.RenewalDate,
            command.PaymentReference,
            command.PaymentMethod,
            command.PaymentAmount);

        if (!renewResult.IsSuccess)
            return Result<RenewPolicyResponseDto>.Fail(renewResult.Error!.Code, renewResult.Error.Message);

        await _policyRepository.SaveChangesAsync();

        return Result<RenewPolicyResponseDto>.Success(new RenewPolicyResponseDto(policy.ToDto()));
    }
}

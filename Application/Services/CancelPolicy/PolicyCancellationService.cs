using Application.Dtos.Response;
using Application.Models.Request;
using Domain.Enums;
using Domain.Repository;
using Domain.ValueObjects;
using SharedKernel;

namespace Application.Services.CancelPolicy;

public class PolicyCancellationService : IPolicyCancellationService
{
    private readonly IPolicyRepository _policyRepository;

    public PolicyCancellationService(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<Result<CancelPolicyResponseDto>> CancelPolicyAsync(string policyReference, CancelPolicyRequestDto request)
    {
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var refundMethod))
            return Result<CancelPolicyResponseDto>.Fail("payment.invalid_type", "Payment type is invalid.");

        var policy = await _policyRepository.GetByReferenceAsync(PolicyReference.FromString(policyReference));

        if (policy is null)
            return Result<CancelPolicyResponseDto>.Fail("policy.not_found", $"Policy with reference {policyReference} does not exist.");

        var cancellationResult = policy.Cancel(request.CancellationDate, refundMethod);
        if (!cancellationResult.IsSuccess)
            return Result<CancelPolicyResponseDto>.Fail(cancellationResult.Error!.Code, cancellationResult.Error.Message);

        await _policyRepository.SaveChangesAsync();

        return Result<CancelPolicyResponseDto>.Success(new CancelPolicyResponseDto(policy.Reference.Value, cancellationResult.Value, refundMethod.ToString()));
    }
}

using Application.Dtos.Response;
using Application.Mappers;
using Application.Models.Command;
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

    public async Task<Result<CancelPolicyResponseDto>> CancelPolicyAsync(string policyReference, CancelPolicyCommand command)
    {
        var policyResult = await GetPolicy(policyReference);
        if (!policyResult.IsSuccess)
            return Result<CancelPolicyResponseDto>.Fail(policyResult.Error!.Code, policyResult.Error.Message);

        var policy = policyResult.Value!;

        var cancellationResult = policy.Cancel(command.CancellationDate, command.RefundMethod);
        if (!cancellationResult.IsSuccess)
            return Result<CancelPolicyResponseDto>.Fail(cancellationResult.Error!.Code, cancellationResult.Error.Message);

        await _policyRepository.SaveChangesAsync();

        return Result<CancelPolicyResponseDto>.Success(
            new CancelPolicyResponseDto(policy.Reference.Value, cancellationResult.Value, command.RefundMethod.ToString()));
    }

    public async Task<Result<CancelPolicyResponseDto>> GetCancellationQuoteAsync(string policyReference, CancelPolicyCommand command)
    {
        var policyResult = await GetPolicy(policyReference);
        if (!policyResult.IsSuccess)
            return Result<CancelPolicyResponseDto>.Fail(policyResult.Error!.Code, policyResult.Error.Message);

        var policy = policyResult.Value!;

        var quoteResult = policy.CalculateCancellationQuote(command.CancellationDate, command.RefundMethod);
        if (!quoteResult.IsSuccess)
            return Result<CancelPolicyResponseDto>.Fail(quoteResult.Error!.Code, quoteResult.Error.Message);

        return Result<CancelPolicyResponseDto>.Success(
            new CancelPolicyResponseDto(policy.Reference.Value, quoteResult.Value, command.RefundMethod.ToString()));
    }

    public async Task<Result<PolicyDto>> MarkAsClaimAsync(string policyReference)
    {
        var policyResult = await GetPolicy(policyReference);
        if (!policyResult.IsSuccess)
            return Result<PolicyDto>.Fail(policyResult.Error!.Code, policyResult.Error.Message);

        var policy = policyResult.Value!;
        var markResult = policy.MarkAsClaim();
        if (!markResult.IsSuccess)
            return Result<PolicyDto>.Fail(markResult.Error!.Code, markResult.Error.Message);

        await _policyRepository.SaveChangesAsync();

        return Result<PolicyDto>.Success(policy.ToDto());
    }

    private async Task<Result<Domain.Aggregates.Policy>> GetPolicy(string policyReference)
    {
        var policy = await _policyRepository.GetByReferenceAsync(PolicyReference.FromString(policyReference));

        if (policy is null)
            return Result<Domain.Aggregates.Policy>.Fail("policy.not_found", $"Policy with reference {policyReference} does not exist.");

        return Result<Domain.Aggregates.Policy>.Success(policy);
    }
}

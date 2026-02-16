using Application.Dtos.Response;
using Application.Models.Request;
using Domain.Aggregates;
using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Domain.ValueObjects;
using SharedKernel;

namespace Application.Services.SellPolicy;

public class PolicySalesService : IPolicySalesService
{
    private readonly IPolicyRepository _policyRepository;

    public PolicySalesService(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<Result<SellPolicyResponseDto>> SellPolicyAsync(SellPolicyRequestDto request)
    {
        var policyResult = Policy.CreateNew(Enum.Parse<HomeInsuranceType>(request.InsuranceType), request.StartDate, Money.Create(request.Amount), request.Property.AddressLine1, request.Property.Postcode, request.AutoRenew);

        if (!policyResult.IsSuccess)
            return Result<SellPolicyResponseDto>.Fail(policyResult.Error.Code, policyResult.Error.Message);

        var policy = policyResult.Value!;

        List<Policyholder> policyHolders = new();

        foreach (var holder in request.Policyholders)
        {
            var policyHolderResult = policy.AddPolicyHolder(holder.FirstName, holder.LastName, holder.DateOfBirth);

            if (!policyHolderResult.IsSuccess)
                return Result<SellPolicyResponseDto>.Fail(policyHolderResult.Error.Code, policyHolderResult.Error.Message);
        }

        if (request.Payment is not null)
        {
            if (!Enum.TryParse<PaymentMethod>(request.Payment.PaymentMethod, ignoreCase: true, out var paymentMethod))
                return Result<SellPolicyResponseDto>.Fail("payment.invalid_type", "Payment type is invalid.");

            var paymentResult = policy.AddPayment(request.Payment.Reference, paymentMethod, request.Payment.Amount);
            if (!paymentResult.IsSuccess)
                return Result<SellPolicyResponseDto>.Fail(paymentResult.Error.Code, paymentResult.Error.Message);
        }

        var purchaseResult = policy.Purchase();
        if (!purchaseResult.IsSuccess)
            return Result<SellPolicyResponseDto>.Fail(purchaseResult.Error.Code, purchaseResult.Error.Message);

        await _policyRepository.Add(policy);
        await _policyRepository.SaveChangesAsync();

        return Result<SellPolicyResponseDto>.Success(new SellPolicyResponseDto(policy.Reference.Value));
    }
}

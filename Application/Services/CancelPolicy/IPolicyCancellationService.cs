using Application.Dtos.Response;
using Application.Models.Command;
using SharedKernel;

namespace Application.Services.CancelPolicy;

public interface IPolicyCancellationService
{
    Task<Result<CancelPolicyResponseDto>> CancelPolicyAsync(string policyReference, CancelPolicyCommand command);
    Task<Result<CancelPolicyResponseDto>> GetCancellationQuoteAsync(string policyReference, CancelPolicyCommand command);
    Task<Result<PolicyDto>> MarkAsClaimAsync(string policyReference);
}

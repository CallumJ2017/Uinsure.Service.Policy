using Application.Dtos.Response;
using Application.Models.Request;
using SharedKernel;

namespace Application.Services.CancelPolicy;

public interface IPolicyCancellationService
{
    Task<Result<CancelPolicyResponseDto>> CancelPolicyAsync(string policyReference, CancelPolicyRequestDto request);
}

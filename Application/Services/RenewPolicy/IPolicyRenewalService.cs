using Application.Dtos.Response;
using Application.Models.Request;
using SharedKernel;

namespace Application.Services.RenewPolicy;

public interface IPolicyRenewalService
{
    Task<Result<RenewPolicyResponseDto>> RenewPolicyAsync(string policyReference, RenewPolicyRequestDto request);
}

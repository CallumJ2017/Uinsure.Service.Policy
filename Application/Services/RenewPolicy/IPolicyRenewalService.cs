using Application.Dtos.Response;
using Application.Models.Command;
using SharedKernel;

namespace Application.Services.RenewPolicy;

public interface IPolicyRenewalService
{
    Task<Result<RenewPolicyResponseDto>> RenewPolicyAsync(string policyReference, RenewPolicyCommand command);
}

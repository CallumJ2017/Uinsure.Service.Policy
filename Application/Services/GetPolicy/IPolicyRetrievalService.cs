using Application.Dtos.Response;
using SharedKernel;

namespace Application.Services.GetPolicy;

public interface IPolicyRetrievalService
{
    Task<Result<PolicyDto>> GetPolicyAsync(string policyReference);
}

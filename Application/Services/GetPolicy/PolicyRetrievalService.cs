using Application.Dtos;
using Application.Dtos.Response;
using Application.Mappers;
using Domain.Repository;
using Domain.ValueObjects;
using SharedKernel;

namespace Application.Services.GetPolicy;

public class PolicyRetrievalService : IPolicyRetrievalService
{
    private readonly IPolicyRepository _policyRepository;

    public PolicyRetrievalService(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<Result<PolicyDto>> GetPolicyAsync(string policyReference)
    {
        var policy = await _policyRepository.GetByReferenceAsync(PolicyReference.FromString(policyReference));

        if (policy is null)
            return Result<PolicyDto>.Fail("policy.not_found", $"Policy with reference {policyReference} does not exist.");

        var dto = policy.ToDto();

        return Result<PolicyDto>.Success(dto);
    }
}

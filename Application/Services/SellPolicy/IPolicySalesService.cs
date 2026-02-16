using Application.Dtos.Response;
using Application.Models.Request;
using SharedKernel;

namespace Application.Services.SellPolicy;

public interface IPolicySalesService
{
    Task<Result<SellPolicyResponseDto>> SellPolicyAsync(SellPolicyRequestDto request);
}

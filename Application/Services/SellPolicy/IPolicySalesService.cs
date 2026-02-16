using Application.Dtos.Response;
using Application.Models.Command;
using SharedKernel;

namespace Application.Services.SellPolicy;

public interface IPolicySalesService
{
    Task<Result<SellPolicyResponseDto>> SellPolicyAsync(SellPolicyCommand command);
}

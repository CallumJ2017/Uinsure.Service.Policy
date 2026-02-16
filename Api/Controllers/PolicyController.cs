using Application.Dtos.Response;
using Application.Models.Request;
using Application.Services.SellPolicy;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/policy")]
public class PolicyController : ControllerBase
{
    private readonly IValidator<SellPolicyRequestDto> _sellPolicyRequestDtoValidator;
    private readonly IPolicySalesService _policySalesService;

    public PolicyController(
        IValidator<SellPolicyRequestDto> sellPolicyRequestDtoValidator,
        IPolicySalesService policySalesService)
    {
        _sellPolicyRequestDtoValidator = sellPolicyRequestDtoValidator;
        _policySalesService = policySalesService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePolicy(SellPolicyRequestDto request)
    {
        var validationResult = await _sellPolicyRequestDtoValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errorResponse = Result<SellPolicyResponseDto>.Fail("request.validation", string.Join(",", validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
            return BadRequest(errorResponse);
        }

        var policyResult = await _policySalesService.SellPolicyAsync(request);

        if (!policyResult.IsSuccess)
        {
            // We could map the different error codes to different Http status codes here.
            // This keeps the HttpStatus codes out of the application.
            return BadRequest(policyResult);
        }

        return Ok(policyResult);
    }
}

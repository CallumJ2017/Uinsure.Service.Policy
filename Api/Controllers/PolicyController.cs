using Application.Dtos.Response;
using Application.Models.Request;
using Application.Services.CancelPolicy;
using Application.Services.GetPolicy;
using Application.Services.RenewPolicy;
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
    private readonly IValidator<CancelPolicyRequestDto> _cancelPolicyRequestDtoValidator;
    private readonly IValidator<RenewPolicyRequestDto> _renewPolicyRequestDtoValidator;
    private readonly IValidator<SellPolicyRequestDto> _sellPolicyRequestDtoValidator;
    private readonly IPolicyCancellationService _policyCancellationService;
    private readonly IPolicyRenewalService _policyRenewalService;
    private readonly IPolicySalesService _policySalesService;
    private readonly IPolicyRetrievalService _policyRetrievalService;

    public PolicyController(
        IValidator<CancelPolicyRequestDto> cancelPolicyRequestDtoValidator,
        IValidator<RenewPolicyRequestDto> renewPolicyRequestDtoValidator,
        IValidator<SellPolicyRequestDto> sellPolicyRequestDtoValidator,
        IPolicyCancellationService policyCancellationService,
        IPolicyRenewalService policyRenewalService,
        IPolicySalesService policySalesService,
        IPolicyRetrievalService policyRetrievalService)
    {
        _cancelPolicyRequestDtoValidator = cancelPolicyRequestDtoValidator;
        _renewPolicyRequestDtoValidator = renewPolicyRequestDtoValidator;
        _sellPolicyRequestDtoValidator = sellPolicyRequestDtoValidator;
        _policyCancellationService = policyCancellationService;
        _policyRenewalService = policyRenewalService;
        _policySalesService = policySalesService;
        _policyRetrievalService = policyRetrievalService;
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

    [HttpGet("{policyReference}")]
    public async Task<IActionResult> GetPolicy(string policyReference)
    {
        var policyResult = await _policyRetrievalService.GetPolicyAsync(policyReference);

        if (!policyResult.IsSuccess && policyResult.Error?.Code == "policy.not_found")
            return NotFound(policyResult);

        if (!policyResult.IsSuccess)
            return BadRequest(policyResult);

        return Ok(policyResult);
    }

    [HttpPost("{policyReference}/cancel")]
    public async Task<IActionResult> CancelPolicy(string policyReference, CancelPolicyRequestDto request)
    {
        var validationResult = await _cancelPolicyRequestDtoValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errorResponse = Result<CancelPolicyResponseDto>.Fail("request.validation", string.Join(",", validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
            return BadRequest(errorResponse);
        }

        var cancellationResult = await _policyCancellationService.CancelPolicyAsync(policyReference, request);

        if (!cancellationResult.IsSuccess && cancellationResult.Error?.Code == "policy.not_found")
            return NotFound(cancellationResult);

        if (!cancellationResult.IsSuccess)
            return BadRequest(cancellationResult);

        return Ok(cancellationResult);
    }

    [HttpPost("{policyReference}/renew")]
    public async Task<IActionResult> RenewPolicy(string policyReference, RenewPolicyRequestDto request)
    {
        var validationResult = await _renewPolicyRequestDtoValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errorResponse = Result<RenewPolicyResponseDto>.Fail("request.validation", string.Join(",", validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
            return BadRequest(errorResponse);
        }

        var renewalResult = await _policyRenewalService.RenewPolicyAsync(policyReference, request);

        if (!renewalResult.IsSuccess && renewalResult.Error?.Code == "policy.not_found")
            return NotFound(renewalResult);

        if (!renewalResult.IsSuccess)
            return BadRequest(renewalResult);

        return Ok(renewalResult);
    }
}
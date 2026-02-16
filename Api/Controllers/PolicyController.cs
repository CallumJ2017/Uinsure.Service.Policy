using Application.Dtos.Response;
using Application.Models.Command;
using Application.Models.Request;
using Application.Services.CancelPolicy;
using Application.Services.GetPolicy;
using Application.Services.RenewPolicy;
using Application.Services.SellPolicy;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using System.Net;

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
    [ProducesResponseType(typeof(Result<SellPolicyResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Result<SellPolicyResponseDto>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreatePolicy(SellPolicyRequestDto request)
    {
        var validationResult = await _sellPolicyRequestDtoValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errorResponse = Result<SellPolicyResponseDto>.Fail("request.validation", string.Join(",", validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
            return BadRequest(errorResponse);
        }

        if (!Enum.TryParse<HomeInsuranceType>(request.InsuranceType, ignoreCase: true, out var insuranceType))
        {
            var errorResponse = Result<SellPolicyResponseDto>.Fail("policy.invalid_insurance_type", "Insurance type is invalid.");
            return BadRequest(errorResponse);
        }

        var command = new SellPolicyCommand
        {
            InsuranceType = insuranceType,
            StartDate = request.StartDate,
            AutoRenew = request.AutoRenew,
            Amount = request.Amount,
            Policyholders = request.Policyholders,
            Property = request.Property,
            Payment = request.Payment
        };

        var policyResult = await _policySalesService.SellPolicyAsync(command);

        if (!policyResult.IsSuccess)
        {
            // We could map the different error codes to different Http status codes here.
            // This keeps the HttpStatus codes out of the application.
            return BadRequest(policyResult);
        }

        return Ok(policyResult);
    }

    [HttpGet("{policyReference}")]
    [ProducesResponseType(typeof(Result<PolicyDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Result<PolicyDto>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(Result<PolicyDto>), (int)HttpStatusCode.BadRequest)]
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
    [ProducesResponseType(typeof(Result<CancelPolicyResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Result<CancelPolicyResponseDto>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(Result<CancelPolicyResponseDto>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CancelPolicy(string policyReference, CancelPolicyRequestDto request)
    {
        var validationResult = await _cancelPolicyRequestDtoValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errorResponse = Result<CancelPolicyResponseDto>.Fail("request.validation", string.Join(",", validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
            return BadRequest(errorResponse);
        }

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var refundMethod))
        {
            var errorResponse = Result<CancelPolicyResponseDto>.Fail("payment.invalid_type", "Payment type is invalid.");
            return BadRequest(errorResponse);
        }

        var command = new CancelPolicyCommand
        {
            CancellationDate = request.CancellationDate,
            RefundMethod = refundMethod
        };

        var cancellationResult = await _policyCancellationService.CancelPolicyAsync(policyReference, command);

        if (!cancellationResult.IsSuccess && cancellationResult.Error?.Code == "policy.not_found")
            return NotFound(cancellationResult);

        if (!cancellationResult.IsSuccess)
            return BadRequest(cancellationResult);

        return Ok(cancellationResult);
    }

    [HttpPost("{policyReference}/cancellation-quote")]
    [ProducesResponseType(typeof(Result<CancelPolicyResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Result<CancelPolicyResponseDto>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(Result<CancelPolicyResponseDto>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CancellationQuote(string policyReference, CancelPolicyRequestDto request)
    {
        var validationResult = await _cancelPolicyRequestDtoValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errorResponse = Result<CancelPolicyResponseDto>.Fail("request.validation", string.Join(",", validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
            return BadRequest(errorResponse);
        }

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var refundMethod))
        {
            var errorResponse = Result<CancelPolicyResponseDto>.Fail("payment.invalid_type", "Payment type is invalid.");
            return BadRequest(errorResponse);
        }

        var command = new CancelPolicyCommand
        {
            CancellationDate = request.CancellationDate,
            RefundMethod = refundMethod
        };

        var quoteResult = await _policyCancellationService.GetCancellationQuoteAsync(policyReference, command);

        if (!quoteResult.IsSuccess && quoteResult.Error?.Code == "policy.not_found")
            return NotFound(quoteResult);

        if (!quoteResult.IsSuccess)
            return BadRequest(quoteResult);

        return Ok(quoteResult);
    }

    [HttpPut("{policyReference}/mark-as-claim")]
    [ProducesResponseType(typeof(Result<PolicyDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Result<PolicyDto>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(Result<PolicyDto>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> MarkAsClaim(string policyReference)
    {
        var claimResult = await _policyCancellationService.MarkAsClaimAsync(policyReference);

        if (!claimResult.IsSuccess && claimResult.Error?.Code == "policy.not_found")
            return NotFound(claimResult);

        if (!claimResult.IsSuccess)
            return BadRequest(claimResult);

        return Ok(claimResult);
    }

    [HttpPost("{policyReference}/renew")]
    [ProducesResponseType(typeof(Result<RenewPolicyResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Result<RenewPolicyResponseDto>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(Result<RenewPolicyResponseDto>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> RenewPolicy(string policyReference, RenewPolicyRequestDto request)
    {
        var validationResult = await _renewPolicyRequestDtoValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errorResponse = Result<RenewPolicyResponseDto>.Fail("request.validation", string.Join(",", validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
            return BadRequest(errorResponse);
        }

        PaymentMethod? paymentMethod = null;

        if (request.Payment is not null)
        {
            if (!Enum.TryParse<PaymentMethod>(request.Payment.PaymentMethod, ignoreCase: true, out var parsedPaymentMethod))
            {
                var errorResponse = Result<RenewPolicyResponseDto>.Fail("payment.invalid_type", "Payment type is invalid.");
                return BadRequest(errorResponse);
            }

            paymentMethod = parsedPaymentMethod;
        }

        var command = new RenewPolicyCommand
        {
            RenewalDate = request.RenewalDate,
            PaymentReference = request.Payment?.Reference,
            PaymentMethod = paymentMethod,
            PaymentAmount = request.Payment?.Amount
        };

        var renewalResult = await _policyRenewalService.RenewPolicyAsync(policyReference, command);

        if (!renewalResult.IsSuccess && renewalResult.Error?.Code == "policy.not_found")
            return NotFound(renewalResult);

        if (!renewalResult.IsSuccess)
            return BadRequest(renewalResult);

        return Ok(renewalResult);
    }
}

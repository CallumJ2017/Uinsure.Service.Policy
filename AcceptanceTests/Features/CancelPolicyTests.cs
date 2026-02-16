using System.Net;
using AcceptanceTests.Dtos;
using AcceptanceTests.Fixtures;
using FluentAssertions;
using RestSharp;

namespace AcceptanceTests.Features;

public class CancelPolicyTests : IClassFixture<HttpClientFixture>
{
    private readonly HttpClientFixture _httpClientFixture;

    public CancelPolicyTests(HttpClientFixture httpClientFixture)
    {
        _httpClientFixture = httpClientFixture;
    }

    [Fact]
    public async Task When_CancelPolicy_BeforeStartDate_Expect_FullRefund()
    {
        var createRequest = BuildValidCreateRequest(startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));
        var postResponse = await _httpClientFixture.Client.ExecuteAsync<Result<SellPolicyResponseDto>>(
            new RestRequest("api/v1/policy", Method.Post).AddJsonBody(createRequest));

        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        postResponse.Data.Should().NotBeNull();
        postResponse.Data!.IsSuccess.Should().BeTrue();
        var policyReference = postResponse.Data.Value!.PolicyNumber;

        var cancelRequest = new CancelPolicyRequestDto
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod = "Card"
        };

        var cancelResponse = await _httpClientFixture.Client.ExecuteAsync<Result<CancelPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/cancel", Method.Post).AddJsonBody(cancelRequest));

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        cancelResponse.Data.Should().NotBeNull();
        cancelResponse.Data!.IsSuccess.Should().BeTrue();
        cancelResponse.Data.Value!.PolicyNumber.Should().Be(policyReference);
        cancelResponse.Data.Value.RefundAmount.Should().Be(createRequest.Amount);
        cancelResponse.Data.Value.RefundPaymentMethod.Should().Be("Card");
    }

    [Fact]
    public async Task When_CancelPolicy_WithDifferentPaymentMethod_Expect_BadRequest()
    {
        var createRequest = BuildValidCreateRequest(startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)));
        var postResponse = await _httpClientFixture.Client.ExecuteAsync<Result<SellPolicyResponseDto>>(
            new RestRequest("api/v1/policy", Method.Post).AddJsonBody(createRequest));

        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var policyReference = postResponse.Data!.Value!.PolicyNumber;

        var cancelRequest = new CancelPolicyRequestDto
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod = "DirectDebit"
        };

        var cancelResponse = await _httpClientFixture.Client.ExecuteAsync<Result<CancelPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/cancel", Method.Post).AddJsonBody(cancelRequest));

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        cancelResponse.Data.Should().NotBeNull();
        cancelResponse.Data!.IsSuccess.Should().BeFalse();
        cancelResponse.Data.Error.Should().NotBeNull();
        cancelResponse.Data.Error!.Code.Should().Be("policy.refund.invalid_method");
    }

    [Fact]
    public async Task When_RequestingCancellationQuote_Expect_RefundAndPolicyNotCancelled()
    {
        var createRequest = BuildValidCreateRequest(startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));
        var postResponse = await _httpClientFixture.Client.ExecuteAsync<Result<SellPolicyResponseDto>>(
            new RestRequest("api/v1/policy", Method.Post).AddJsonBody(createRequest));

        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var policyReference = postResponse.Data!.Value!.PolicyNumber;

        var quoteRequest = new CancelPolicyRequestDto
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod = "Card"
        };

        var quoteResponse = await _httpClientFixture.Client.ExecuteAsync<Result<CancelPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/cancellation-quote", Method.Post).AddJsonBody(quoteRequest));

        quoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        quoteResponse.Data!.IsSuccess.Should().BeTrue();
        quoteResponse.Data.Value!.RefundAmount.Should().Be(createRequest.Amount);

        var getResponse = await _httpClientFixture.Client.ExecuteAsync<Result<PolicyDto>>(
            new RestRequest($"api/v1/policy/{policyReference}", Method.Get));

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Data!.Value!.Status.Should().Be("Active");
    }

    [Fact]
    public async Task When_CancelPolicy_AfterClaim_Expect_NoRefund()
    {
        var createRequest = BuildValidCreateRequest(startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));
        var postResponse = await _httpClientFixture.Client.ExecuteAsync<Result<SellPolicyResponseDto>>(
            new RestRequest("api/v1/policy", Method.Post).AddJsonBody(createRequest));

        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var policyReference = postResponse.Data!.Value!.PolicyNumber;

        var claimResponse = await _httpClientFixture.Client.ExecuteAsync<Result<PolicyDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/mark-as-claim", Method.Put));

        claimResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        claimResponse.Data!.IsSuccess.Should().BeTrue();
        claimResponse.Data.Value!.HasClaims.Should().BeTrue();

        var cancelRequest = new CancelPolicyRequestDto
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod = "DirectDebit"
        };

        var cancelResponse = await _httpClientFixture.Client.ExecuteAsync<Result<CancelPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/cancel", Method.Post).AddJsonBody(cancelRequest));

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        cancelResponse.Data!.IsSuccess.Should().BeTrue();
        cancelResponse.Data.Value!.RefundAmount.Should().Be(0m);
    }

    private static SellPolicyRequestDto BuildValidCreateRequest(DateOnly startDate)
    {
        return new SellPolicyRequestDto
        {
            InsuranceType = "Household",
            StartDate = startDate,
            AutoRenew = false,
            Amount = 180.75m,
            Policyholders =
            [
                new PolicyholderDto
                {
                    FirstName = "Alex",
                    LastName = "Smith",
                    DateOfBirth = new DateOnly(1991, 3, 11)
                }
            ],
            Property = new PropertyDto
            {
                AddressLine1 = "9 Cancellation Lane",
                AddressLine2 = "Test Town",
                AddressLine3 = null,
                Postcode = "TT11AA"
            },
            Payment = new PaymentDto
            {
                Reference = $"PAY-{Guid.NewGuid():N}",
                PaymentMethod = "Card",
                Amount = 180.75m
            }
        };
    }
}

using System.Net;
using AcceptanceTests.Dtos;
using AcceptanceTests.Fixtures;
using FluentAssertions;
using RestSharp;

namespace AcceptanceTests.Features;

public class PolicyRetrievalTests : IClassFixture<HttpClientFixture>
{
    private readonly HttpClientFixture _httpClientFixture;

    public PolicyRetrievalTests(HttpClientFixture httpClientFixture)
    {
        _httpClientFixture = httpClientFixture;
    }

    [Fact]
    public async Task When_GetPolicy_WithExistingPolicyReference_Expect_MappedPolicyDto()
    {
        var createRequest = BuildValidCreateRequest();

        var postResponse = await _httpClientFixture.Client.ExecuteAsync<Result<SellPolicyResponseDto>>(
            new RestRequest("api/v1/policy", Method.Post).AddJsonBody(createRequest));

        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        postResponse.Data.Should().NotBeNull();
        postResponse.Data!.IsSuccess.Should().BeTrue();
        postResponse.Data.Value.Should().NotBeNull();

        var policyReference = postResponse.Data.Value!.PolicyNumber;

        var getResponse = await _httpClientFixture.Client.ExecuteAsync<Result<PolicyDto>>(
            new RestRequest($"api/v1/policy/{policyReference}", Method.Get));

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Data.Should().NotBeNull();
        getResponse.Data!.IsSuccess.Should().BeTrue();
        getResponse.Data.Value.Should().NotBeNull();

        var policy = getResponse.Data.Value!;
        policy.Reference.Should().Be(policyReference);
        policy.InsuranceType.Should().Be("Household");
        policy.Status.Should().Be("Active");
        policy.Amount.Should().Be(createRequest.Amount);
        policy.Property.Should().NotBeNull();
        policy.Property!.AddressLine1.Should().Be(createRequest.Property.AddressLine1);
        policy.Policyholders.Should().HaveCount(1);
        policy.Payments.Should().HaveCount(1);
        policy.Payments.Single().Reference.Should().Be(createRequest.Payment.Reference);
        policy.Payments.Single().PaymentMethod.Should().Be("Card");
    }

    [Fact]
    public async Task When_GetPolicy_WithUnknownPolicyRefernce_Expect_NotFound()
    {
        var getResponse = await _httpClientFixture.Client.ExecuteAsync<Result<PolicyDto>>(
            new RestRequest($"api/v1/policy/{Guid.NewGuid()}", Method.Get));

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        getResponse.Data.Should().NotBeNull();
        getResponse.Data!.IsSuccess.Should().BeFalse();
        getResponse.Data.Error.Should().NotBeNull();
        getResponse.Data.Error!.Code.Should().Be("policy.not_found");
    }

    private static SellPolicyRequestDto BuildValidCreateRequest()
    {
        return new SellPolicyRequestDto
        {
            InsuranceType = "Household",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            AutoRenew = false,
            Amount = 220.40m,
            Policyholders =
            [
                new PolicyholderDto
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    DateOfBirth = new DateOnly(1988, 8, 15)
                }
            ],
            Property = new PropertyDto
            {
                AddressLine1 = "2 Retrieval Road",
                AddressLine2 = "Test Area",
                AddressLine3 = null,
                Postcode = "ZX12YY"
            },
            Payment = new PaymentDto
            {
                Reference = $"PAY-{Guid.NewGuid():N}",
                PaymentMethod = "Card",
                Amount = 220.40m
            }
        };
    }
}

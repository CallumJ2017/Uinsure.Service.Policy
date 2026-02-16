using AcceptanceTests.Dtos;
using AcceptanceTests.Fixtures;
using FluentAssertions;
using RestSharp;

namespace AcceptanceTests.Features;

public class SellPolicyTests : IClassFixture<HttpClientFixture>
{
    private readonly HttpClientFixture _httpClientFixture;

    public SellPolicyTests(HttpClientFixture httpClientFixture)
    {
        _httpClientFixture = httpClientFixture;
    }

    [Fact]
    public async Task When_CreatePolicy_With_ValidRequest_Expect_SuccessfulResponseWithPolicyNumber()
    {
        var requestDto = new SellPolicyRequestDto
        {
            InsuranceType = "Household",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            AutoRenew = false,
            Amount = 120.50m,
            Policyholders = new List<PolicyholderDto>
            {
                new PolicyholderDto
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateOnly(1990, 5, 12)
                }
            },
            Property = new PropertyDto
            {
                AddressLine1 = "1 Test Street",
                AddressLine2 = "Test Area",
                AddressLine3 = null,
                Postcode = "AB12CD"
            },
        };

        var request = new RestRequest($"api/v1/policy", Method.Post).AddJsonBody(requestDto);
        var response = await _httpClientFixture.Client.ExecuteAsync<Result<SellPolicyResponseDto>>(request);

        response.Data.IsSuccess.Should().BeTrue();
        response.Data.Value.Should().NotBeNull();
    }
}

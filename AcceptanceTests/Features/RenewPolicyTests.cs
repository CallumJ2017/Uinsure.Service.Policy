using System.Net;
using AcceptanceTests.Dtos;
using AcceptanceTests.Fixtures;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using RestSharp;

namespace AcceptanceTests.Features;

public class RenewPolicyTests : IClassFixture<HttpClientFixture>
{
    private const string SqlConnectionString = "Server=localhost,1433;Database=Policy;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True;";

    private readonly HttpClientFixture _httpClientFixture;

    public RenewPolicyTests(HttpClientFixture httpClientFixture)
    {
        _httpClientFixture = httpClientFixture;
    }

    [Fact]
    public async Task When_Renewing_Before30DayWindow_Expect_BadRequest()
    {
        var policyReference = await CreatePolicy(autoRenew: true);
        await UpdatePolicyDatesAsync(policyReference, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-350));
        var policy = await GetPolicy(policyReference);

        var renewRequest = new RenewPolicyRequestDto
        {
            RenewalDate = policy.EndDate.AddDays(-31),
            Payment = new PaymentDto
            {
                Reference = $"PAY-{Guid.NewGuid():N}",
                PaymentMethod = "Card",
                Amount = 100m
            }
        };

        var renewResponse = await _httpClientFixture.Client.ExecuteAsync<Result<RenewPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/renew", Method.Post).AddJsonBody(renewRequest));

        renewResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        renewResponse.Data.Should().NotBeNull();
        renewResponse.Data!.IsSuccess.Should().BeFalse();
        renewResponse.Data.Error!.Code.Should().Be("policy.renewal.too_early");
    }

    [Fact]
    public async Task When_Renewing_AfterEndDate_Expect_BadRequest()
    {
        var policyReference = await CreatePolicy(autoRenew: true);
        await UpdatePolicyDatesAsync(policyReference, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-370));
        var policy = await GetPolicy(policyReference);

        var renewRequest = new RenewPolicyRequestDto
        {
            RenewalDate = policy.EndDate.AddDays(1),
            Payment = new PaymentDto
            {
                Reference = $"PAY-{Guid.NewGuid():N}",
                PaymentMethod = "Card",
                Amount = 100m
            }
        };

        var renewResponse = await _httpClientFixture.Client.ExecuteAsync<Result<RenewPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/renew", Method.Post).AddJsonBody(renewRequest));

        renewResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        renewResponse.Data.Should().NotBeNull();
        renewResponse.Data!.IsSuccess.Should().BeFalse();
        renewResponse.Data.Error!.Code.Should().Be("policy.renewal.after_end_date");
    }

    [Fact]
    public async Task When_Renewing_AutoRenewTrue_Expect_PaymentCreated()
    {
        var policyReference = await CreatePolicy(autoRenew: true);
        await UpdatePolicyDatesAsync(policyReference, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-350));
        var policyBefore = await GetPolicy(policyReference);

        var renewRequest = new RenewPolicyRequestDto
        {
            RenewalDate = policyBefore.EndDate.AddDays(-5),
            Payment = new PaymentDto
            {
                Reference = $"PAY-{Guid.NewGuid():N}",
                PaymentMethod = "Card",
                Amount = 100m
            }
        };

        var renewResponse = await _httpClientFixture.Client.ExecuteAsync<Result<RenewPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/renew", Method.Post).AddJsonBody(renewRequest));

        renewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        renewResponse.Data.Should().NotBeNull();
        renewResponse.Data!.IsSuccess.Should().BeTrue();
        renewResponse.Data.Value.Should().NotBeNull();
        renewResponse.Data.Value!.Policy.Payments.Count.Should().Be(policyBefore.Payments.Count + 1);
    }

    [Fact]
    public async Task When_Renewing_AutoRenewFalse_Expect_PaymentNotCreated()
    {
        var policyReference = await CreatePolicy(autoRenew: false);
        await UpdatePolicyDatesAsync(policyReference, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-350));
        var policyBefore = await GetPolicy(policyReference);

        var renewRequest = new RenewPolicyRequestDto
        {
            RenewalDate = policyBefore.EndDate.AddDays(-5),
            Payment = new PaymentDto
            {
                Reference = $"PAY-{Guid.NewGuid():N}",
                PaymentMethod = "Card",
                Amount = 100m
            }
        };

        var renewResponse = await _httpClientFixture.Client.ExecuteAsync<Result<RenewPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/renew", Method.Post).AddJsonBody(renewRequest));

        renewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        renewResponse.Data.Should().NotBeNull();
        renewResponse.Data!.IsSuccess.Should().BeTrue();
        renewResponse.Data.Value.Should().NotBeNull();
        renewResponse.Data.Value!.Policy.Payments.Count.Should().Be(policyBefore.Payments.Count);
    }

    [Fact]
    public async Task When_Renewing_AutoRenewTrue_WithChequePayment_Expect_BadRequest()
    {
        var policyReference = await CreatePolicy(autoRenew: true);
        await UpdatePolicyDatesAsync(policyReference, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-350));
        var policyBefore = await GetPolicy(policyReference);

        var renewRequest = new RenewPolicyRequestDto
        {
            RenewalDate = policyBefore.EndDate.AddDays(-5),
            Payment = new PaymentDto
            {
                Reference = $"PAY-{Guid.NewGuid():N}",
                PaymentMethod = "Cheque",
                Amount = 100m
            }
        };

        var renewResponse = await _httpClientFixture.Client.ExecuteAsync<Result<RenewPolicyResponseDto>>(
            new RestRequest($"api/v1/policy/{policyReference}/renew", Method.Post).AddJsonBody(renewRequest));

        renewResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        renewResponse.Data.Should().NotBeNull();
        renewResponse.Data!.IsSuccess.Should().BeFalse();
        renewResponse.Data.Error!.Code.Should().Be("policy.renewal.cheque_not_allowed");
    }

    private async Task<string> CreatePolicy(bool autoRenew)
    {
        var requestDto = new SellPolicyRequestDto
        {
            InsuranceType = "Household",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            AutoRenew = autoRenew,
            Amount = 120.50m,
            Policyholders =
            [
                new PolicyholderDto
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateOnly(1990, 5, 12)
                }
            ],
            Property = new PropertyDto
            {
                AddressLine1 = "1 Test Street",
                AddressLine2 = "Test Area",
                AddressLine3 = null,
                Postcode = "AB12CD"
            },
            Payment = new PaymentDto
            {
                Reference = $"PAY-{Guid.NewGuid():N}",
                PaymentMethod = "Card",
                Amount = 120.50m
            }
        };

        var response = await _httpClientFixture.Client.ExecuteAsync<Result<SellPolicyResponseDto>>(
            new RestRequest("api/v1/policy", Method.Post).AddJsonBody(requestDto));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.IsSuccess.Should().BeTrue();

        return response.Data.Value!.PolicyNumber;
    }

    private static async Task UpdatePolicyDatesAsync(string policyReference, DateOnly startDate)
    {
        await using var connection = new SqlConnection(SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE Policies
            SET StartDate = @startDate,
                EndDate = DATEADD(year, 1, @startDate)
            WHERE Reference = @policyReference
            """,
            connection);

        command.Parameters.AddWithValue("@startDate", startDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@policyReference", policyReference);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        rowsAffected.Should().Be(1);
    }

    private async Task<PolicyDto> GetPolicy(string policyReference)
    {
        var response = await _httpClientFixture.Client.ExecuteAsync<Result<PolicyDto>>(
            new RestRequest($"api/v1/policy/{policyReference}", Method.Get));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.IsSuccess.Should().BeTrue();

        return response.Data.Value!;
    }
}

using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace UnitTests.Domain;

public class PolicyAggregateTests
{
    private readonly string _reference = "POL123.UIN.B2L";
    private readonly DateOnly _validStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
    private readonly Money _validPremium = Money.Create(500m, "GBP");
    private readonly Property _validProperty = Property.Create("1 Test Street", "AB12 3CD").Value!;
    private readonly bool _autoRenew = true;

    private Policyholder CreateValidPolicyholder(string firstName = "John", string lastName = "Doe")
    {
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20));
        return Policyholder.Create(firstName, lastName, dob);
    }

    [Fact]
    public void CreateNew_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        var policyholders = new List<Policyholder> { CreateValidPolicyholder() };

        var result = Policy.CreateNew(_reference, _validStartDate, _validPremium, _validProperty, _autoRenew, policyholders);     
        result.IsSuccess.Should().BeTrue();

        var policy = result.Value!;

        policy.Reference.Should().Be(_reference);
        policy.StartDate.Should().Be(_validStartDate);
        policy.EndDate.Should().Be(_validStartDate.AddYears(1));
        policy.Premium.Should().Be(_validPremium);
        policy.HasClaims.Should().BeFalse();
        policy.InsuredProperty.Should().Be(_validProperty);
        policy.PolicyHolders.Count.Should().Be(policyholders.Count);
    }

    [Fact]
    public void CreateNew_ShouldFail_WhenStartDateIsMoreThan60DaysInFuture()
    {
        var policyholders = new List<Policyholder> { CreateValidPolicyholder() };
        var futureStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(61));

        var result = Policy.CreateNew(_reference, futureStartDate, _validPremium, _validProperty, _autoRenew, policyholders);
        result.IsSuccess.Should().BeFalse();

        var error = result.Error!;
        error.Code.Should().Be("policy.start.toofar");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void CreateNew_ShouldFail_WhenPolicyholderCountIsInvalid(int policyholderCount)
    {
        var policyholders = new List<Policyholder>();
        for (int i = 0; i < policyholderCount; i++)
            policyholders.Add(CreateValidPolicyholder());

        var result = Policy.CreateNew(_reference, _validStartDate, _validPremium, _validProperty, _autoRenew, policyholders);
        result.IsSuccess.Should().BeFalse();

        var error = result.Error!;
        error.Code.Should().Be("policy.policyholders.count");
    }

    [Fact]
    public void CreateNew_ShouldFail_WhenPolicyholderIsUnderMinimumAge()
    {
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-15));
        var underagePolicyholder = new Policyholder("Jane", "Doe", dob);
        var policyholders = new List<Policyholder> { underagePolicyholder };

        var result = Policy.CreateNew(_reference, _validStartDate, _validPremium, _validProperty, _autoRenew, policyholders);
        result.IsSuccess.Should().BeFalse();

        var error = result.Error!;
        error.Code.Should().Be("policy.policyholders.minimum_age");
    }
}

using Domain.Aggregates;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace UnitTests.Domain;

public class PolicyAggregateTests
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly HomeInsuranceType _validType = HomeInsuranceType.Household;
    private readonly DateOnly _validStartDate = Today.AddDays(30);
    private readonly bool _autoRenew = true;

    private const string AddressLine1 = "1 Test Street";
    private const string Postcode = "AB12CD"; // <= 8 chars

    private readonly Money _validPremium = Money.Create(500m, "GBP");

    private static DateOnly DobAtLeastAge(DateOnly policyStartDate, int ageYears)
        => policyStartDate.AddYears(-ageYears).AddDays(-1);

    [Fact]
    public void CreateNew_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        var result = Policy.CreateNew(
            _validType,
            _validStartDate,
            _validPremium,
            AddressLine1,
            Postcode,
            _autoRenew);

        result.IsSuccess.Should().BeTrue();
        var policy = result.Value!;

        policy.Status.Should().Be(PolicyStatus.Draft);
        policy.StartDate.Should().Be(_validStartDate);
        policy.EndDate.Should().Be(_validStartDate.AddYears(1));
        policy.AutoRenew.Should().BeTrue();
        policy.HasClaims.Should().BeFalse();

        policy.InsuredProperty.Should().NotBeNull();
        policy.InsuredProperty.AddressLine1.Should().Be(AddressLine1);
        policy.InsuredProperty.Postcode.Should().Be(Postcode);

        policy.PolicyHolders.Should().BeEmpty();
    }

    [Fact]
    public void CreateNew_ShouldFail_WhenStartDateIsMoreThan60DaysInFuture()
    {
        var tooFar = Today.AddDays(61);

        var result = Policy.CreateNew(
            _validType,
            tooFar,
            _validPremium,
            AddressLine1,
            Postcode,
            _autoRenew);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.start.toofar");
    }

    [Fact]
    public void CreateNew_ShouldFail_WhenAddressLine1IsMissing()
    {
        var result = Policy.CreateNew(
            _validType,
            _validStartDate,
            _validPremium,
            addressLine1: "",
            postcode: Postcode,
            _autoRenew);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("property.invalid_address");
    }

    [Fact]
    public void CreateNew_ShouldFail_WhenPostcodeIsMissing()
    {
        var result = Policy.CreateNew(
            _validType,
            _validStartDate,
            _validPremium,
            addressLine1: AddressLine1,
            postcode: "",
            _autoRenew);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("property.invalid_postcode");
    }

    [Fact]
    public void CreateNew_ShouldFail_WhenPostcodeTooLong()
    {
        var result = Policy.CreateNew(
            _validType,
            _validStartDate,
            _validPremium,
            addressLine1: AddressLine1,
            postcode: "TOO-LONG-POSTCODE",
            _autoRenew);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("property.invalid_postcode_length");
    }

    [Fact]
    public void AddPolicyHolder_ShouldAdd_WhenValidAndDraft()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;

        var dob = DobAtLeastAge(_validStartDate, 20);
        var addResult = policy.AddPolicyHolder("John", "Doe", dob);

        addResult.IsSuccess.Should().BeTrue();
        policy.PolicyHolders.Should().HaveCount(1);

        var holder = policy.PolicyHolders.Single();
        holder.FirstName.Should().Be("John");
        holder.LastName.Should().Be("Doe");
        holder.DateOfBirth.Should().Be(dob);
    }

    [Fact]
    public void AddPolicyHolder_ShouldFail_WhenUnderMinimumAgeAtStartDate()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;

        var dob = _validStartDate.AddYears(-15);

        var result = policy.AddPolicyHolder("Jane", "Doe", dob);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.policyholders.minimum_age");
    }

    [Fact]
    public void AddPolicyHolder_ShouldFail_WhenExceedingMaximumPolicyholders()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;
        var dob = DobAtLeastAge(_validStartDate, 30);

        policy.AddPolicyHolder("A", "One", dob).IsSuccess.Should().BeTrue();
        policy.AddPolicyHolder("B", "Two", dob).IsSuccess.Should().BeTrue();
        policy.AddPolicyHolder("C", "Three", dob).IsSuccess.Should().BeTrue();

        var fourth = policy.AddPolicyHolder("D", "Four", dob);

        fourth.IsSuccess.Should().BeFalse();
        fourth.Error!.Code.Should().Be("policy.policyholders.max_count");
    }

    [Fact]
    public void Purchase_ShouldFail_WhenNoPolicyholders()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;

        var purchase = policy.Purchase();

        purchase.IsSuccess.Should().BeFalse();
        purchase.Error!.Code.Should().Be("policy.policyholders.required");
        policy.Status.Should().Be(PolicyStatus.Draft);
    }

    [Fact]
    public void Purchase_ShouldSucceed_WhenAtLeastOnePolicyholderExists()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;
        policy.AddPolicyHolder("John", "Doe", DobAtLeastAge(_validStartDate, 25)).IsSuccess.Should().BeTrue();

        var purchase = policy.Purchase();

        purchase.IsSuccess.Should().BeTrue();
        policy.Status.Should().Be(PolicyStatus.Active);
        policy.LastModifiedAt.Should().NotBe(default);
    }

    [Fact]
    public void Purchase_ShouldFail_WhenNotDraft()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;
        policy.AddPolicyHolder("John", "Doe", DobAtLeastAge(_validStartDate, 25)).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        var secondPurchase = policy.Purchase();

        secondPurchase.IsSuccess.Should().BeFalse();
        secondPurchase.Error!.Code.Should().Be("policy.invalid_state");
    }

    [Fact]
    public void AddPolicyHolder_ShouldFail_WhenPolicyIsNotDraft()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;
        policy.AddPolicyHolder("John", "Doe", DobAtLeastAge(_validStartDate, 25)).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        var result = policy.AddPolicyHolder("Jane", "Doe", DobAtLeastAge(_validStartDate, 30));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.locked");
    }
}
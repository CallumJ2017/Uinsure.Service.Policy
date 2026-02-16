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
    private const string Postcode = "AB12CD";

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
        policy.AddPayment("PAY-PURCHASE-001", PaymentMethod.Card, 10m).IsSuccess.Should().BeTrue();

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
        policy.AddPayment("PAY-PURCHASE-002", PaymentMethod.Card, 10m).IsSuccess.Should().BeTrue();
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
        policy.AddPayment("PAY-PURCHASE-003", PaymentMethod.Card, 10m).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        var result = policy.AddPolicyHolder("Jane", "Doe", DobAtLeastAge(_validStartDate, 30));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.locked");
    }

    [Fact]
    public void AddPayment_ShouldAdd_WhenValidAndDraft()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;

        var result = policy.AddPayment("PAY-123", PaymentMethod.Card, 99.99m);

        result.IsSuccess.Should().BeTrue();
        policy.Payments.Should().HaveCount(1);
        policy.Payments.Single().Reference.Should().Be("PAY-123");
        policy.Payments.Single().Type.Should().Be(PaymentMethod.Card);
        policy.Payments.Single().Amount.Should().Be(99.99m);
    }

    [Fact]
    public void AddPayment_ShouldFail_WhenReferenceIsEmpty()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;

        var result = policy.AddPayment(string.Empty, PaymentMethod.DirectDebit, 42m);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("payment.invalid_reference");
    }

    [Fact]
    public void AddPayment_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;

        var zeroAmount = policy.AddPayment("PAY-001", PaymentMethod.Cheque, 0m);
        var negativeAmount = policy.AddPayment("PAY-002", PaymentMethod.Cheque, -1m);

        zeroAmount.IsSuccess.Should().BeFalse();
        zeroAmount.Error!.Code.Should().Be("payment.invalid_amount");
        negativeAmount.IsSuccess.Should().BeFalse();
        negativeAmount.Error!.Code.Should().Be("payment.invalid_amount");
    }

    [Fact]
    public void AddPayment_ShouldFail_WhenPolicyIsNotDraft()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;
        policy.AddPolicyHolder("John", "Doe", DobAtLeastAge(_validStartDate, 25)).IsSuccess.Should().BeTrue();
        policy.AddPayment("PAY-PURCHASE-004", PaymentMethod.Card, 10m).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        var result = policy.AddPayment("PAY-123", PaymentMethod.Card, 10m);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.locked");
    }

    [Fact]
    public void Cancel_ShouldReturnFullRefund_WhenCancelledBeforePolicyStartDate()
    {
        var startDate = Today.AddDays(30);
        var premium = Money.Create(365m);
        var policy = CreateActivePolicy(startDate, premium, PaymentMethod.Card);

        var result = policy.Cancel(startDate.AddDays(-1), PaymentMethod.Card);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(365m);
        policy.Status.Should().Be(PolicyStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldReturnFullRefund_WhenCancelledWithinCoolingOffPeriod()
    {
        var startDate = Today.AddDays(-5);
        var premium = Money.Create(365m);
        var policy = CreateActivePolicy(startDate, premium, PaymentMethod.Card);

        var result = policy.Cancel(startDate.AddDays(14), PaymentMethod.Card);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(365m);
        policy.Status.Should().Be(PolicyStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldReturnProRataRefund_WhenCancelledAfterCoolingOffPeriod()
    {
        var startDate = Today.AddDays(-40);
        var premium = Money.Create(365m);
        var policy = CreateActivePolicy(startDate, premium, PaymentMethod.Card);
        var cancellationDate = startDate.AddDays(20);

        var totalCoverageDays = policy.EndDate.DayNumber - policy.StartDate.DayNumber;
        var unusedDays = policy.EndDate.DayNumber - cancellationDate.DayNumber;
        var expectedRefund = decimal.Round(premium.Value * unusedDays / totalCoverageDays, 2, MidpointRounding.AwayFromZero);

        var result = policy.Cancel(cancellationDate, PaymentMethod.Card);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedRefund);
        result.Value.Should().BeLessThan(premium.Value);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenRefundMethodDiffersFromOriginalPaymentMethod()
    {
        var policy = CreateActivePolicy(Today.AddDays(-10), Money.Create(250m), PaymentMethod.Card);

        var result = policy.Cancel(Today, PaymentMethod.DirectDebit);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.refund.invalid_method");
    }

    [Fact]
    public void Cancel_ShouldFail_WhenPolicyIsNotActive()
    {
        var policy = Policy.CreateNew(_validType, _validStartDate, _validPremium, AddressLine1, Postcode, _autoRenew).Value!;

        var result = policy.Cancel(Today, PaymentMethod.Card);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.invalid_state");
    }

    [Fact]
    public void Cancel_ShouldFail_WhenPolicyAlreadyCancelled()
    {
        var policy = CreateActivePolicy(Today.AddDays(-10), Money.Create(500m), PaymentMethod.Card);
        policy.Cancel(Today, PaymentMethod.Card).IsSuccess.Should().BeTrue();

        var secondAttempt = policy.Cancel(Today, PaymentMethod.Card);

        secondAttempt.IsSuccess.Should().BeFalse();
        secondAttempt.Error!.Code.Should().Be("policy.invalid_state");
    }

    [Fact]
    public void CalculateCancellationQuote_ShouldReturnRefundWithoutCancellingPolicy()
    {
        var startDate = Today.AddDays(-20);
        var premium = Money.Create(365m);
        var policy = CreateActivePolicy(startDate, premium, PaymentMethod.Card);
        var cancellationDate = startDate.AddDays(16);

        var quote = policy.CalculateCancellationQuote(cancellationDate, PaymentMethod.Card);

        quote.IsSuccess.Should().BeTrue();
        quote.Value.Should().BeLessThan(premium.Value);
        policy.Status.Should().Be(PolicyStatus.Active);
    }

    [Fact]
    public void Cancel_ShouldReturnZeroRefund_WhenPolicyHasClaims()
    {
        var policy = CreateActivePolicy(Today.AddDays(-40), Money.Create(300m), PaymentMethod.Card);
        policy.MarkAsClaim().IsSuccess.Should().BeTrue();

        var result = policy.Cancel(Today, PaymentMethod.DirectDebit);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0m);
    }

    [Fact]
    public void Renew_ShouldFail_WhenEarlierThan30DaysBeforeEndDate()
    {
        var policy = CreateActivePolicy(Today.AddDays(-120), Money.Create(365m), PaymentMethod.Card, autoRenew: true);
        var renewalDate = policy.EndDate.AddDays(-31);

        var result = policy.Renew(renewalDate, "PAY-RENEW-001", PaymentMethod.Card, 10m);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.renewal.too_early");
    }

    [Fact]
    public void Renew_ShouldFail_WhenAfterEndDate()
    {
        var policy = CreateActivePolicy(Today.AddDays(-400), Money.Create(365m), PaymentMethod.Card, autoRenew: true);
        var renewalDate = policy.EndDate.AddDays(1);

        var result = policy.Renew(renewalDate, "PAY-RENEW-002", PaymentMethod.Card, 10m);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.renewal.after_end_date");
    }

    [Fact]
    public void Renew_ShouldCreatePayment_WhenAutoRenewIsTrue()
    {
        var policy = CreateActivePolicy(Today.AddDays(-360), Money.Create(365m), PaymentMethod.Card, autoRenew: true);
        var originalEndDate = policy.EndDate;
        var originalPaymentCount = policy.Payments.Count;

        var result = policy.Renew(originalEndDate.AddDays(-10), "PAY-RENEW-003", PaymentMethod.DirectDebit, 99.99m);

        result.IsSuccess.Should().BeTrue();
        policy.EndDate.Should().Be(originalEndDate.AddYears(1));
        policy.Payments.Should().HaveCount(originalPaymentCount + 1);
        policy.Payments.Last().Reference.Should().Be("PAY-RENEW-003");
        policy.Payments.Last().Type.Should().Be(PaymentMethod.DirectDebit);
        policy.Payments.Last().Amount.Should().Be(99.99m);
    }

    [Fact]
    public void Renew_ShouldNotCreatePayment_WhenAutoRenewIsFalse()
    {
        var policy = CreateActivePolicy(Today.AddDays(-360), Money.Create(365m), PaymentMethod.Card, autoRenew: false);
        var originalEndDate = policy.EndDate;
        var originalPaymentCount = policy.Payments.Count;

        var result = policy.Renew(originalEndDate.AddDays(-10), "PAY-RENEW-004", PaymentMethod.Cheque, 55.55m);

        result.IsSuccess.Should().BeTrue();
        policy.EndDate.Should().Be(originalEndDate.AddYears(1));
        policy.Payments.Should().HaveCount(originalPaymentCount);
    }

    [Fact]
    public void Renew_ShouldFail_WhenAutoRenewIsTrueAndPaymentNotProvided()
    {
        var policy = CreateActivePolicy(Today.AddDays(-360), Money.Create(365m), PaymentMethod.Card, autoRenew: true);

        var result = policy.Renew(policy.EndDate.AddDays(-5), paymentReference: null, paymentMethod: null, paymentAmount: null);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.renewal.payment.required");
    }

    [Fact]
    public void Renew_ShouldFail_WhenAutoRenewIsTrueAndPaymentMethodIsCheque()
    {
        var policy = CreateActivePolicy(Today.AddDays(-360), Money.Create(365m), PaymentMethod.Card, autoRenew: true);

        var result = policy.Renew(policy.EndDate.AddDays(-5), "PAY-RENEW-005", PaymentMethod.Cheque, 12m);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.renewal.cheque_not_allowed");
    }

    private Policy CreateActivePolicy(DateOnly startDate, Money premium, PaymentMethod paymentMethod, bool autoRenew = true)
    {
        var createResult = Policy.CreateNew(_validType, startDate, premium, AddressLine1, Postcode, autoRenew);
        createResult.IsSuccess.Should().BeTrue();

        var policy = createResult.Value!;
        policy.AddPolicyHolder("John", "Doe", DobAtLeastAge(startDate, 30)).IsSuccess.Should().BeTrue();
        policy.AddPayment("PAY-ACTIVE-001", paymentMethod, premium.Value).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        return policy;
    }
}

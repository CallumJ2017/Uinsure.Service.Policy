using Application.Models.Command;
using Application.Services.CancelPolicy;
using Domain.Aggregates;
using Domain.Enums;
using Domain.Repository;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace UnitTests.Application.CancelPolicy;

public class PolicyCancellationServiceTests
{
    [Fact]
    public async Task CancelPolicyAsync_ShouldFail_WhenRefundMethodDiffersFromOriginalPaymentMethod()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(15);
        var createResult = Policy.CreateNew(HomeInsuranceType.Household, startDate, Money.Create(120m), "1 Test St", "AB12CD", autoRenew: true);
        createResult.IsSuccess.Should().BeTrue();

        var policy = createResult.Value!;
        policy.AddPolicyHolder("Jane", "Doe", startDate.AddYears(-30)).IsSuccess.Should().BeTrue();
        policy.AddPayment("PAY-CANCEL-METHOD-001", PaymentMethod.Card, 120m).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        var repository = new Mock<IPolicyRepository>();
        repository.Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>())).ReturnsAsync(policy);
        var sut = new PolicyCancellationService(repository.Object);

        var result = await sut.CancelPolicyAsync(policy.Reference.Value, new CancelPolicyCommand
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RefundMethod = PaymentMethod.DirectDebit
        });

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.refund.invalid_method");
    }

    [Fact]
    public async Task CancelPolicyAsync_ShouldFail_WhenPolicyDoesNotExist()
    {
        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>()))
            .ReturnsAsync((Policy?)null);
        var sut = new PolicyCancellationService(repository.Object);

        var result = await sut.CancelPolicyAsync("HOM-0000001", new CancelPolicyCommand
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RefundMethod = PaymentMethod.Card
        });

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.not_found");
    }

    [Fact]
    public async Task CancelPolicyAsync_ShouldReturnRefund_WhenCancellationIsValid()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(15);
        var createResult = Policy.CreateNew(HomeInsuranceType.Household, startDate, Money.Create(120m), "1 Test St", "AB12CD", autoRenew: true);
        createResult.IsSuccess.Should().BeTrue();

        var policy = createResult.Value!;
        policy.AddPolicyHolder("Jane", "Doe", startDate.AddYears(-30)).IsSuccess.Should().BeTrue();
        policy.AddPayment("PAY-CANCEL-001", PaymentMethod.Card, 120m).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>()))
            .ReturnsAsync(policy);

        var sut = new PolicyCancellationService(repository.Object);

        var result = await sut.CancelPolicyAsync(policy.Reference.Value, new CancelPolicyCommand
        {
            CancellationDate = startDate.AddDays(-1),
            RefundMethod = PaymentMethod.Card
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PolicyNumber.Should().Be(policy.Reference.Value);
        result.Value.RefundAmount.Should().Be(120m);
        result.Value.RefundPaymentMethod.Should().Be("Card");
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCancellationQuoteAsync_ShouldReturnRefund_AndNotPersist()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20);
        var createResult = Policy.CreateNew(HomeInsuranceType.Household, startDate, Money.Create(220m), "1 Test St", "AB12CD", autoRenew: true);
        createResult.IsSuccess.Should().BeTrue();

        var policy = createResult.Value!;
        policy.AddPolicyHolder("Jane", "Doe", startDate.AddYears(-30)).IsSuccess.Should().BeTrue();
        policy.AddPayment("PAY-QUOTE-001", PaymentMethod.Card, 220m).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        var repository = new Mock<IPolicyRepository>();
        repository.Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>())).ReturnsAsync(policy);
        var sut = new PolicyCancellationService(repository.Object);

        var result = await sut.GetCancellationQuoteAsync(policy.Reference.Value, new CancelPolicyCommand
        {
            CancellationDate = startDate.AddDays(-1),
            RefundMethod = PaymentMethod.Card
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.RefundAmount.Should().Be(220m);
        policy.Status.Should().Be(PolicyStatus.Active);
        repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CancelPolicyAsync_ShouldReturnZeroRefund_WhenPolicyHasClaim()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-20);
        var createResult = Policy.CreateNew(HomeInsuranceType.Household, startDate, Money.Create(300m), "1 Test St", "AB12CD", autoRenew: true);
        createResult.IsSuccess.Should().BeTrue();

        var policy = createResult.Value!;
        policy.AddPolicyHolder("Jane", "Doe", startDate.AddYears(-30)).IsSuccess.Should().BeTrue();
        policy.AddPayment("PAY-CANCEL-CLAIM-001", PaymentMethod.Card, 300m).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();
        policy.MarkAsClaim().IsSuccess.Should().BeTrue();

        var repository = new Mock<IPolicyRepository>();
        repository.Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>())).ReturnsAsync(policy);
        var sut = new PolicyCancellationService(repository.Object);

        var result = await sut.CancelPolicyAsync(policy.Reference.Value, new CancelPolicyCommand
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RefundMethod = PaymentMethod.DirectDebit
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.RefundAmount.Should().Be(0m);
    }

    [Fact]
    public async Task MarkAsClaimAsync_ShouldSetHasClaimsAndPersist()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20);
        var createResult = Policy.CreateNew(HomeInsuranceType.Household, startDate, Money.Create(200m), "1 Test St", "AB12CD", autoRenew: true);
        createResult.IsSuccess.Should().BeTrue();

        var policy = createResult.Value!;
        policy.AddPolicyHolder("Jane", "Doe", startDate.AddYears(-30)).IsSuccess.Should().BeTrue();
        policy.AddPayment("PAY-CLAIM-001", PaymentMethod.Card, 200m).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        var repository = new Mock<IPolicyRepository>();
        repository.Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>())).ReturnsAsync(policy);
        var sut = new PolicyCancellationService(repository.Object);

        var result = await sut.MarkAsClaimAsync(policy.Reference.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value!.HasClaims.Should().BeTrue();
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}

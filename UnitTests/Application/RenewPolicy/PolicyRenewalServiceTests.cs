using Application.Models.Command;
using Application.Services.RenewPolicy;
using Domain.Aggregates;
using Domain.Enums;
using Domain.Repository;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace UnitTests.Application.RenewPolicy;

public class PolicyRenewalServiceTests
{
    [Fact]
    public async Task RenewPolicyAsync_ShouldFail_WhenPolicyDoesNotExist()
    {
        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>()))
            .ReturnsAsync((Policy?)null);

        var sut = new PolicyRenewalService(repository.Object);

        var result = await sut.RenewPolicyAsync("HOM-0000001", new RenewPolicyCommand
        {
            RenewalDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.not_found");
    }

    [Fact]
    public async Task RenewPolicyAsync_ShouldSucceed_AndCreatePayment_WhenAutoRenewIsTrue()
    {
        var policy = BuildActivePolicy(autoRenew: true);
        var originalPaymentCount = policy.Payments.Count;
        var originalEndDate = policy.EndDate;
        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>()))
            .ReturnsAsync(policy);

        var sut = new PolicyRenewalService(repository.Object);

        var result = await sut.RenewPolicyAsync(policy.Reference.Value, new RenewPolicyCommand
        {
            RenewalDate = policy.EndDate.AddDays(-5),
            PaymentReference = "PAY-RENEW-002",
            PaymentMethod = PaymentMethod.Card,
            PaymentAmount = 77.77m
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Policy.EndDate.Should().Be(originalEndDate.AddYears(1));
        result.Value.Policy.Payments.Should().HaveCount(originalPaymentCount + 1);
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RenewPolicyAsync_ShouldSucceed_AndNotCreatePayment_WhenAutoRenewIsFalse()
    {
        var policy = BuildActivePolicy(autoRenew: false);
        var originalPaymentCount = policy.Payments.Count;
        var originalEndDate = policy.EndDate;
        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>()))
            .ReturnsAsync(policy);

        var sut = new PolicyRenewalService(repository.Object);

        var result = await sut.RenewPolicyAsync(policy.Reference.Value, new RenewPolicyCommand
        {
            RenewalDate = policy.EndDate.AddDays(-5),
            PaymentReference = "PAY-RENEW-003",
            PaymentMethod = PaymentMethod.Card,
            PaymentAmount = 120m
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Policy.EndDate.Should().Be(originalEndDate.AddYears(1));
        result.Value.Policy.Payments.Should().HaveCount(originalPaymentCount);
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RenewPolicyAsync_ShouldFail_WhenAutoRenewTrueAndPaymentMethodIsCheque()
    {
        var policy = BuildActivePolicy(autoRenew: true);
        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>()))
            .ReturnsAsync(policy);

        var sut = new PolicyRenewalService(repository.Object);

        var result = await sut.RenewPolicyAsync(policy.Reference.Value, new RenewPolicyCommand
        {
            RenewalDate = policy.EndDate.AddDays(-5),
            PaymentReference = "PAY-RENEW-004",
            PaymentMethod = PaymentMethod.Cheque,
            PaymentAmount = 120m
        });

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("policy.renewal.cheque_not_allowed");
        repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    private static Policy BuildActivePolicy(bool autoRenew)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-350);
        var createResult = Policy.CreateNew(HomeInsuranceType.Household, startDate, Money.Create(200m), "1 Test St", "AB12CD", autoRenew);
        createResult.IsSuccess.Should().BeTrue();

        var policy = createResult.Value!;
        policy.AddPolicyHolder("Jane", "Doe", startDate.AddYears(-30)).IsSuccess.Should().BeTrue();
        policy.AddPayment("PAY-INITIAL-001", PaymentMethod.Card, 200m).IsSuccess.Should().BeTrue();
        policy.Purchase().IsSuccess.Should().BeTrue();

        return policy;
    }
}

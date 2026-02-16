using Application.Models.Request;
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
    public async Task CancelPolicyAsync_ShouldFail_WhenPaymentMethodIsInvalid()
    {
        var repository = new Mock<IPolicyRepository>();
        var sut = new PolicyCancellationService(repository.Object);

        var result = await sut.CancelPolicyAsync("HOM-0000001", new CancelPolicyRequestDto
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod = "InvalidMethod"
        });

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("payment.invalid_type");
    }

    [Fact]
    public async Task CancelPolicyAsync_ShouldFail_WhenPolicyDoesNotExist()
    {
        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(x => x.GetByReferenceAsync(It.IsAny<PolicyReference>()))
            .ReturnsAsync((Policy?)null);
        var sut = new PolicyCancellationService(repository.Object);

        var result = await sut.CancelPolicyAsync("HOM-0000001", new CancelPolicyRequestDto
        {
            CancellationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod = "Card"
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

        var result = await sut.CancelPolicyAsync(policy.Reference.Value, new CancelPolicyRequestDto
        {
            CancellationDate = startDate.AddDays(-1),
            PaymentMethod = "Card"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PolicyNumber.Should().Be(policy.Reference.Value);
        result.Value.RefundAmount.Should().Be(120m);
        result.Value.RefundPaymentMethod.Should().Be("Card");
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}

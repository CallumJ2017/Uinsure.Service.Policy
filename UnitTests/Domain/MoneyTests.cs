using Domain.ValueObjects;
using FluentAssertions;
using SharedKernel;

namespace UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Create_ShouldReturnMoney_WhenValidValueAndCurrencyProvided()
    {
        decimal value = 100m;
        string currency = "GBP";

        var money = Money.Create(value, currency);

        money.Value.Should().Be(value);
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Create_ShouldDefaultCurrencyToGBP_WhenCurrencyNotProvided()
    {
        decimal value = 50m;

        var money = Money.Create(value);

        money.Value.Should().Be(value);
        money.Currency.Should().Be("GBP");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ShouldThrowDomainException_WhenValueIsNegativeOrZero(decimal invalidValue)
    {
        Action act = () => Money.Create(invalidValue);

        act.Should().Throw<DomainException>().WithMessage("Value must be greater than 0.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldThrowDomainException_WhenCurrencyIsNullOrEmpty(string? invalidCurrency)
    {
        Action act = () => Money.Create(100, invalidCurrency!);

        act.Should().Throw<DomainException>().WithMessage("Currency is required.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForMoneyWithSameValueAndCurrency()
    {
        var money1 = Money.Create(100, "GBP");
        var money2 = Money.Create(100, "GBP");

        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForMoneyWithDifferentValueOrCurrency()
    {
        var money1 = Money.Create(100, "GBP");
        var money2 = Money.Create(200, "GBP");
        var money3 = Money.Create(100, "USD");

        money1.Should().NotBe(money2);
        money1.Should().NotBe(money3);
    }
}
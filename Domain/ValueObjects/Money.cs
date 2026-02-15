using SharedKernel;

namespace Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Value { get; }
    public string Currency { get; }

    internal Money(decimal value, string currency = "GBP")
    {
        Guard.AgainstNegative(value, "policy.invalid_amount", "Value must be greater than 0.");
        Guard.AgainstNullOrEmpty(currency, "policy.invalid_currency", "Currency is required.");

        Value = value;
        Currency = currency;
    }

    public static Money Create(decimal value, string currency = "GBP") => new(value, currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Currency;
    }
}
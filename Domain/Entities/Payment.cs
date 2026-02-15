using Domain.ValueObjects;
using SharedKernel;

namespace Domain.Entities;

public sealed class Payment : Entity<Guid>
{
    public string PaymentReference { get; private set; }
    public string Type { get; private set; }
    public Money Amount { get; private set; }

    internal Payment(Guid id, string paymentReference, string type, Money amount) : base(id)
    {
        Guard.AgainstNullOrEmpty(paymentReference, "payment.invalid_reference", "Payment reference is required.");
        Guard.AgainstNullOrEmpty(type, "payment.invalid_type", "Payment type is required.");
        Guard.AgainstNull(amount, "payment.invalid_amount", "Payment amount is required.");

        PaymentReference = paymentReference;
        Type = type;
        Amount = amount;
    }
}
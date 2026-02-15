using Domain.ValueObjects;
using SharedKernel;

namespace Domain.Entities;

public sealed class Refund : Entity<Guid>
{
    public string RefundReference { get; private set; }
    public string Type { get; private set; }
    public Money Amount { get; private set; }

    internal Refund(Guid id, string refundReference, string type, Money amount) : base(id)
    {
        Guard.AgainstNullOrEmpty(refundReference, "refund.invalid_reference", "Refund reference is required.");
        Guard.AgainstNullOrEmpty(type, "refund.invalid_type", "Refund type is required.");
        Guard.AgainstNull(amount, "refund.invalid_amount", "Refund amount is required.");

        RefundReference = refundReference;
        Type = type;
        Amount = amount;
    }
}


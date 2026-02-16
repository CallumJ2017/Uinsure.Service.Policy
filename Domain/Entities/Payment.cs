using Domain.Enums;
using SharedKernel;

namespace Domain.Entities;

public sealed class Payment : Entity<Guid>
{
    public string Reference { get; private set; }
    public PaymentMethod Type { get; private set; }
    public decimal Amount { get; private set; }

    protected Payment() { }

    private Payment(string reference, PaymentMethod type, decimal amount) : base(Guid.NewGuid())
    {
        Guard.AgainstNullOrEmpty(reference, "payment.invalid_reference", "Payment reference is required.");

        if (amount <= 0)
            throw new DomainException("payment.invalid_amount", "Payment amount must be greater than zero.");

        Reference = reference;
        Type = type;
        Amount = amount;
    }

    public static Result<Payment> Create(string reference, PaymentMethod type, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return Result<Payment>.Fail("payment.invalid_reference", "Payment reference is required.");

        if (amount <= 0)
            return Result<Payment>.Fail("payment.invalid_amount", "Payment amount must be greater than zero.");

        return Result<Payment>.Success(new Payment(reference, type, amount));
    }
}

using Domain.Enums;

namespace Application.Models.Command;

public class RenewPolicyCommand
{
    public DateOnly RenewalDate { get; set; }
    public string? PaymentReference { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal? PaymentAmount { get; set; }
}

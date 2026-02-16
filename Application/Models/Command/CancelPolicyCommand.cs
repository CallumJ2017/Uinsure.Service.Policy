using Domain.Enums;

namespace Application.Models.Command;

public class CancelPolicyCommand
{
    public DateOnly CancellationDate { get; set; }
    public PaymentMethod RefundMethod { get; set; }
}

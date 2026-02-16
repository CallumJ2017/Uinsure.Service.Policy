using Application.Dtos;
using Domain.Enums;

namespace Application.Models.Command;

public class SellPolicyCommand
{
    public HomeInsuranceType InsuranceType { get; set; }
    public DateOnly StartDate { get; set; }
    public bool AutoRenew { get; set; }
    public decimal Amount { get; set; }
    public required List<PolicyholderDto> Policyholders { get; set; }
    public required PropertyDto Property { get; set; }
    public PaymentDto? Payment { get; set; }
}

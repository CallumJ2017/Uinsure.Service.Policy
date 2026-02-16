using Application.Dtos;

namespace Application.Models.Request;

public class SellPolicyRequestDto
{
    public required string InsuranceType { get; set; }
    public DateOnly StartDate { get; set; }
    public bool AutoRenew { get; set; }
    public decimal Amount { get; set; }
    public required List<PolicyholderDto> Policyholders { get; set; }
    public required PropertyDto Property { get; set; }
    public PaymentDto? Payment { get; set; }
}

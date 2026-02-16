namespace AcceptanceTests.Dtos;

public class SellPolicyRequestDto
{
    public string InsuranceType { get; set; }
    public DateOnly StartDate { get; set; }
    public bool AutoRenew { get; set; }
    public decimal Amount { get; set; }
    public List<PolicyholderDto> Policyholders { get; set; }
    public PropertyDto Property { get; set; }
    public PaymentDto Payment { get; set; }
}

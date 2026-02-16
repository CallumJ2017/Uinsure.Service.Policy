namespace AcceptanceTests.Dtos;

public class PolicyDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; }
    public string InsuranceType { get; set; }
    public string Status { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal Amount { get; set; }
    public bool AutoRenew { get; set; }
    public bool HasClaims { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastModifiedAt { get; set; }
    public PropertyDto Property { get; set; }
    public List<PolicyholderDto> Policyholders { get; set; }
    public List<PaymentDto> Payments { get; set; }
}

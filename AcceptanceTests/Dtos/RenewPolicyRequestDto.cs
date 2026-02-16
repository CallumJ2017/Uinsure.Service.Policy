namespace AcceptanceTests.Dtos;

public class RenewPolicyRequestDto
{
    public DateOnly RenewalDate { get; set; }
    public PaymentDto? Payment { get; set; }
}

namespace AcceptanceTests.Dtos;

public class CancelPolicyResponseDto
{
    public string PolicyNumber { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundPaymentMethod { get; set; }
}

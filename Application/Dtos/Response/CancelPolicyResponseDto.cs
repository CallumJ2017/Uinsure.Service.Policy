namespace Application.Dtos.Response;

public class CancelPolicyResponseDto
{
    public CancelPolicyResponseDto(string policyNumber, decimal refundAmount, string refundPaymentMethod)
    {
        PolicyNumber = policyNumber;
        RefundAmount = refundAmount;
        RefundPaymentMethod = refundPaymentMethod;
    }

    public string PolicyNumber { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundPaymentMethod { get; set; }
}

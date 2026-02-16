namespace Application.Models.Request;

public class CancelPolicyRequestDto
{
    public DateOnly CancellationDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

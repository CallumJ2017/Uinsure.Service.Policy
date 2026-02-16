namespace AcceptanceTests.Dtos;

public class CancelPolicyRequestDto
{
    public DateOnly CancellationDate { get; set; }
    public string PaymentMethod { get; set; }
}

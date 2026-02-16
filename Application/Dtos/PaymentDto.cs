namespace Application.Dtos;

public class PaymentDto
{
    public string Reference { get; set; }
    public string PaymentMethod { get; set; }
    public decimal Amount { get; set; }
}

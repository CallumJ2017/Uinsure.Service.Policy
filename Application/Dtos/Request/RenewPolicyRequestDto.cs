using Application.Dtos;

namespace Application.Models.Request;

public class RenewPolicyRequestDto
{
    public DateOnly RenewalDate { get; set; }
    public PaymentDto? Payment { get; set; }
}

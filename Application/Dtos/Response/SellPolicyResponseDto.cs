namespace Application.Dtos.Response;

public class SellPolicyResponseDto
{
    public SellPolicyResponseDto(string policyNumber)
    {
        PolicyNumber = policyNumber;
    }

    public string PolicyNumber { get; set; }
}

namespace Application.Dtos.Response;

public class RenewPolicyResponseDto
{
    public RenewPolicyResponseDto(PolicyDto policy)
    {
        Policy = policy;
    }

    public PolicyDto Policy { get; set; }
}

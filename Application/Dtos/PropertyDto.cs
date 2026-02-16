namespace Application.Dtos;

public class PropertyDto
{
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public required string Postcode { get; set; }
}

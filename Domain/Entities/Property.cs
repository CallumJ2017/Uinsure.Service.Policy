using SharedKernel;

namespace Domain.Entities;

public sealed class Property : Entity<Guid>
{
    public string AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? AddressLine3 { get; private set;  }
    public string Postcode { get; private set; }

    protected Property() { }

    internal Property(string addressLine1, string postcode, string? addressLine2 = null, string? addressLine3 = null) : base(Guid.NewGuid())
    {
        Guard.AgainstNullOrEmpty(addressLine1, "property.invalid_address", "Address Line 1 is required.");
        Guard.AgainstNullOrEmpty(postcode, "property.invalid_postcode", "Postcode is required.");

        if (postcode.Length > 8)
            throw new DomainException("property.invalid_postcode_length", "Postcode cannot exceed 8 characters.");

        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        AddressLine3 = addressLine3;
        Postcode = postcode;
    }

    public static Result<Property> Create(
        string addressLine1,
        string postcode,
        string? addressLine2 = null,
        string? addressLine3 = null)
    {
        if (string.IsNullOrWhiteSpace(addressLine1))
            return Result<Property>.Fail("property.invalid_address", "Address Line 1 is required.");

        if (string.IsNullOrWhiteSpace(postcode))
            return Result<Property>.Fail("property.invalid_postcode", "Postcode is required.");

        if (postcode.Length > 8)
            return Result<Property>.Fail("property.invalid_postcode_length", "Postcode cannot exceed 8 characters.");

        return Result<Property>.Success(new Property(addressLine1, postcode, addressLine2, addressLine3));
    }

    public override string ToString()
    {
        return string.Join(", ", new[] { AddressLine1, AddressLine2, AddressLine3, Postcode }
            .Where(line => !string.IsNullOrWhiteSpace(line)));
    }
}
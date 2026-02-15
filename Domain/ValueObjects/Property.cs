using SharedKernel;

namespace Domain.ValueObjects;

public sealed class Property : ValueObject
{
    public string AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? AddressLine3 { get; private set;  }
    public string Postcode { get; private set; }

    internal Property(string addressLine1, string postcode, string? addressLine2 = null, string? addressLine3 = null)
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

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AddressLine1;
        yield return AddressLine2 ?? string.Empty;
        yield return AddressLine3 ?? string.Empty;
        yield return Postcode;
    }

    public override string ToString()
    {
        return string.Join(", ", new[] { AddressLine1, AddressLine2, AddressLine3, Postcode }
            .Where(line => !string.IsNullOrWhiteSpace(line)));
    }
}
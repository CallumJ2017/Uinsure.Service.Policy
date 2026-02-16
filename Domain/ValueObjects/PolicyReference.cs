using Domain.Enums;
using SharedKernel;

namespace Domain.ValueObjects;

public sealed class PolicyReference : ValueObject
{
    public string Value { get; }

    private PolicyReference(string value)
    {
        Value = value;
    }

    public static PolicyReference Generate(HomeInsuranceType type)
    {
        var reference = PolicyReferenceGenerator.Generate(type);
        return new PolicyReference(reference);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static PolicyReference FromString(string value) => new PolicyReference(value);
}
using Domain.ValueObjects;
using FluentAssertions;

namespace UnitTests.Domain;

public class PropertyTests
{
    private readonly string _validAddressLine1 = "1 Test address line 1";
    private readonly string _validPostcode = "AB12 3CD";
    private readonly string _validAddressLine2 = "Test address line 2";
    private readonly string _validAddressLine3 = "Test address line 3";

    [Fact]
    public void CreateResult_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        var result = Property.Create(_validAddressLine1, _validPostcode, _validAddressLine2, _validAddressLine3);

        result.IsSuccess.Should().BeTrue();

        var property = result.Value!;
        property.AddressLine1.Should().Be(_validAddressLine1);
        property.Postcode.Should().Be(_validPostcode);
        property.AddressLine2.Should().Be(_validAddressLine2);
        property.AddressLine3.Should().Be(_validAddressLine3);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenAddressLine1IsEmptyOrNull(string? addressLine1)
    {
        var result = Property.Create(addressLine1!, _validPostcode);

        result.IsSuccess.Should().BeFalse();

        var error = result.Error!;
        error.Code.Should().Be("property.invalid_address");
        error.Message.Should().Be("Address Line 1 is required.");
    }

    [Fact]
    public void Create_ShouldFail_WhenPostcodeIsTooLong()
    {
        var postcode = "AB12345678"; // 10 chars

        var result = Property.Create(_validAddressLine1, postcode);

        result.IsSuccess.Should().BeFalse();

        var error = result.Error!;
        error.Code.Should().Be("property.invalid_postcode_length");
    }

    [Fact]
    public void ToString_ShouldReturnInCorrectFormat_WhenDataIsValid()
    {
        var expectedOutput = $"{_validAddressLine1}, {_validAddressLine2}, {_validAddressLine3}, {_validPostcode}";

        var result = Property.Create(_validAddressLine1, _validPostcode, _validAddressLine2, _validAddressLine3);

        result.IsSuccess.Should().BeTrue();

        var property = result.Value!;

        property.ToString().Should().Be(expectedOutput);
    }
}
using Domain.Entities;
using FluentAssertions;

namespace UnitTests.Domain;

public class PropertyTests
{
    private const string ValidAddressLine1 = "1 Test address line 1";
    private const string ValidPostcode = "AB12 3CD";
    private const string ValidAddressLine2 = "Test address line 2";
    private const string ValidAddressLine3 = "Test address line 3";

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        var result = Property.Create(
            ValidAddressLine1,
            ValidPostcode,
            ValidAddressLine2,
            ValidAddressLine3);

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();

        var property = result.Value!;
        property.AddressLine1.Should().Be(ValidAddressLine1);
        property.Postcode.Should().Be(ValidPostcode);
        property.AddressLine2.Should().Be(ValidAddressLine2);
        property.AddressLine3.Should().Be(ValidAddressLine3);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenAddressLine1IsEmptyOrNull(string? addressLine1)
    {
        var result = Property.Create(addressLine1 ?? string.Empty, ValidPostcode);

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();

        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("property.invalid_address");
        result.Error.Message.Should().Be("Address Line 1 is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenPostcodeIsEmptyOrNull(string? postcode)
    {
        var result = Property.Create(ValidAddressLine1, postcode ?? string.Empty);

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();

        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("property.invalid_postcode");
        result.Error.Message.Should().Be("Postcode is required.");
    }

    [Fact]
    public void Create_ShouldFail_WhenPostcodeIsTooLong()
    {
        var tooLongPostcode = "AB12345678";

        var result = Property.Create(ValidAddressLine1, tooLongPostcode);

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();

        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("property.invalid_postcode_length");
        result.Error.Message.Should().Be("Postcode cannot exceed 8 characters.");
    }

    [Fact]
    public void ToString_ShouldReturnAllPartsSeparatedByComma_WhenAllFieldsProvided()
    {
        var property = Property.Create(
            ValidAddressLine1,
            ValidPostcode,
            ValidAddressLine2,
            ValidAddressLine3).Value!;

        property.ToString().Should().Be($"{ValidAddressLine1}, {ValidAddressLine2}, {ValidAddressLine3}, {ValidPostcode}");
    }

    [Fact]
    public void ToString_ShouldOmitNullOrWhitespaceAddressLines()
    {
        var property = Property.Create(
            ValidAddressLine1,
            ValidPostcode,
            addressLine2: null,
            addressLine3: " ").Value!;

        property.ToString().Should().Be($"{ValidAddressLine1}, {ValidPostcode}");
    }
}
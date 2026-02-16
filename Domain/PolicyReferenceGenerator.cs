using Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Domain;

public static class PolicyReferenceGenerator
{
    public static string Generate(HomeInsuranceType type)
    {
        var prefix = type switch
        {
            HomeInsuranceType.Household => "HH",
            HomeInsuranceType.BuyToLet => "B2L",
            _ => "XX"
        };

        var randomPart = RandomAlphanumeric(5);

        var baseReference = $"POL-{prefix}-{randomPart}";

        var checksum = CalculateChecksum(baseReference);

        return $"{baseReference}-{checksum}";
    }

    private static string RandomAlphanumeric(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(0, chars.Length);
            result[i] = chars[idx];
        }
        return new string(result);
    }

    private static int CalculateChecksum(string input)
    {
        // Simple: sum of ASCII bytes modulo 10
        var bytes = Encoding.ASCII.GetBytes(input);
        int sum = 0;
        foreach (var b in bytes)
            sum += b;
        return sum % 10;
    }
}


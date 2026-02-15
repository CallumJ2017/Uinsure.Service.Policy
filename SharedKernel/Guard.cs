namespace SharedKernel;

public static class Guard
{
    public static void AgainstNullOrEmpty(string value, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(code, message);
    }

    public static void AgainstDefault<T>(T value, string code, string message) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
            throw new DomainException(code, message);
    }

    public static void AgainstNegative(decimal value, string code, string message)
    {
        if (value <= 0)
            throw new DomainException(code, message);
    }

    public static void AgainstNull(object? value, string code, string message)
    {
        if (value is null)
            throw new DomainException(code, message);
    }
}
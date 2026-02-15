namespace Domain;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, Error? error = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public Error? Error { get; set; }

    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Fail(string code, string message) => new(false, default, new Error(code, message));
}

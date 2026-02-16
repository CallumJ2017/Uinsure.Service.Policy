namespace SharedKernel;

public sealed class Result<T>
{
    public Result() { }

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

public sealed class Result
{
    public Result() { }

    private Result(bool isSuccess, Error? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; private set; }
    public Error? Error { get; set; }

    public static Result Success() => new(true);
    public static Result Fail(string code, string message) => new(false, new Error(code, message));
}
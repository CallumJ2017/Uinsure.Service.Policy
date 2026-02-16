namespace AcceptanceTests.Dtos;

public sealed class Result<T>
{
    public Result() { }

    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public ErrorDto? Error { get; set; }
}
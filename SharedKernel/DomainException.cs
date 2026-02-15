namespace SharedKernel;

public class DomainException : Exception
{
    public string Code { get; private set; }

    public DomainException(string code, string message) : base(message) 
    {
        Code = code;
    }
}
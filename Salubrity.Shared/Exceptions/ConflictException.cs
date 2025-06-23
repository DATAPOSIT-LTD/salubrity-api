namespace Salubrity.Shared.Exceptions;

public class ConflictException : BaseAppException
{
    public ConflictException(string message = "A conflict occurred.", string? errorCode = null)
        : base(message, 409, errorCode ?? "conflict") { }
}

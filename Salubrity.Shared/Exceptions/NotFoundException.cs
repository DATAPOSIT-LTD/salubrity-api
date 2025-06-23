namespace Salubrity.Shared.Exceptions;

public class NotFoundException : BaseAppException
{
    public NotFoundException(string message = "Resource not found.", string? errorCode = null)
        : base(message, 404, errorCode ?? "not_found") { }
}


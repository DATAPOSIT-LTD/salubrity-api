namespace Salubrity.Shared.Exceptions;

public class ValidationException : BaseAppException
{
    public ValidationException(List<string> errors)
        : base("Validation failed.", 400, "validation_error", errors)
    {
    }
}

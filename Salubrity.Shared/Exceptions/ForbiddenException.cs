namespace Salubrity.Shared.Exceptions;

public class ForbiddenException : BaseAppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, 403, "forbidden")
    {
    }
}

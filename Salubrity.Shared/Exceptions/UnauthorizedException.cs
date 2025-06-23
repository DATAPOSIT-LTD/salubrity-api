namespace Salubrity.Shared.Exceptions;

public class UnauthorizedException : BaseAppException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message, 401, "unauthorized")
    {
    }
}

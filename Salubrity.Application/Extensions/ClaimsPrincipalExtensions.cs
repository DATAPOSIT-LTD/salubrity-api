// Salubrity.Application/Extensions/ClaimsPrincipalExtensions.cs
using System.Security.Claims;
using Salubrity.Shared.Exceptions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var v = user.FindFirst("user_id")?.Value
                 ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (v is null || !Guid.TryParse(v, out var id))
            throw new UnauthorizedException("You are not authorized to access this resource.");
        return id;
    }
}

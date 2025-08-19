using System.Security.Claims;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetSubcontractorId(this ClaimsPrincipal user)
        {
            var subcontractorIdClaim = user.FindFirst("subcontractor_id");
            if (subcontractorIdClaim == null || !Guid.TryParse(subcontractorIdClaim.Value, out var subcontractorId))
            {
                throw new UnauthorizedException("Subcontractor ID is missing or invalid. Did you forget to login first?");
            }
            return subcontractorId;
        }
    }
}
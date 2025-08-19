using System.Security.Claims;

namespace Salubrity.Application.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetSubcontractorId(this ClaimsPrincipal user)
        {
            var subcontractorIdClaim = user.FindFirst("subcontractor_id");
            if (subcontractorIdClaim == null || !Guid.TryParse(subcontractorIdClaim.Value, out var subcontractorId))
            {
                throw new InvalidOperationException("Subcontractor ID claim is missing or invalid.");
            }
            return subcontractorId;
        }
    }
}
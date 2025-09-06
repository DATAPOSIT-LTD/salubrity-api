// File: Salubrity.Application/DTOs/HealthCamps/CampLinkResult.cs

namespace Salubrity.Application.DTOs.HealthCamps
{
    public class CampLinkResultDto
    {
        public bool Linked { get; set; }                  // Was the user successfully linked?
        public Guid? CampId { get; set; }                 // The ID of the camp (if found from token)
        public List<string> Warnings { get; set; } = [];  // Non-fatal issues (e.g., "camp has ended")
        public List<string> Info { get; set; } = [];      // Helpful info (e.g., "already linked")
    }
}

namespace Salubrity.Application.DTOs.Subcontractor
{
    public class CreateSubcontractorRoleDto
    {
        /// <summary>
        /// Name of the role (e.g., "Doctor", "Nurse", "Support Staff")
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Optional description for the role
        /// </summary>
        public string? Description { get; set; }
    }
}

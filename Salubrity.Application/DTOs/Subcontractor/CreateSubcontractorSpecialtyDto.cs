namespace Salubrity.Application.DTOs.Subcontractor;

public class CreateSubcontractorSpecialtyDto
{
    public Guid SubcontractorId { get; set; }
    public Guid ServiceId { get; set; } // The specialty being added
}

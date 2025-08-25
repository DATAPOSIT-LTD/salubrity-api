namespace Salubrity.Application.DTOs.HealthCamps
{
    public class HealthCampPatientDto
    {
        public string PatientId { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Company { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
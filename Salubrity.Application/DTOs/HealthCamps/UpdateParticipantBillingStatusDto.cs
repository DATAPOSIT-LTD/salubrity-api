using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.HealthCamps
{
    public class UpdateParticipantBillingStatusDto
    {
        [Required]
        public Guid BillingStatusId { get; set; }
    }
}

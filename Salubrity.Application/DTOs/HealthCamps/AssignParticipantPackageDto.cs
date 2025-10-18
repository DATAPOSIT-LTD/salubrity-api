using System;

namespace Salubrity.Application.DTOs.HealthCamps
{
    public class AssignParticipantPackageDto
    {
        public Guid HealthCampId { get; set; }
        public Guid ParticipantId { get; set; }
        public Guid HealthCampPackageId { get; set; }
    }
}

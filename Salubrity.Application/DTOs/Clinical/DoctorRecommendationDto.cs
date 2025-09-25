// File: Application/DTOs/Clinical/DoctorRecommendationDto.cs
using System;
using Salubrity.Application.DTOs.Lookups;

namespace Salubrity.Application.DTOs.Clinical
{
    public class CreateDoctorRecommendationDto
    {
        public Guid PatientId { get; set; }
        public Guid HealthCampId { get; set; }

        // New fields
        public string? PertinentHistoryFindings { get; set; }
        public string? PertinentClinicalFindings { get; set; }
        public string? DiagnosticImpression { get; set; }
        public string? Conclusion { get; set; }

        public Guid FollowUpRecommendationId { get; set; }
        public Guid RecommendationTypeId { get; set; }
        public string? Instructions { get; set; }
    }

    public class UpdateDoctorRecommendationDto : CreateDoctorRecommendationDto
    {
        public Guid Id { get; set; }
    }

    public class DoctorRecommendationResponseDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid HealthCampId { get; set; }

        // New fields
        public string? PertinentHistoryFindings { get; set; }
        public string? PertinentClinicalFindings { get; set; }
        public string? DiagnosticImpression { get; set; }
        public string? Conclusion { get; set; }

        public BaseLookupResponse FollowUpRecommendation { get; set; } = default!;
        public BaseLookupResponse RecommendationType { get; set; } = default!;
        public string? Instructions { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

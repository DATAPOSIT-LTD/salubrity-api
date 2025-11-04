using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthAssessments;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Application.Interfaces.Services.HealthAssessments;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class CampDataFetcher
    {
        private readonly IHealthCampRepository _healthCampRepository;
        private readonly IIntakeFormResponseRepository _intakeFormResponseRepository;
        private readonly IHealthAssessmentFormService _healthAssessmentFormService;
        private readonly IDoctorRecommendationService _doctorRecommendationService;


        public CampDataFetcher(
            IHealthCampRepository healthCampRepository,
            IIntakeFormResponseRepository intakeFormResponseRepository,
            IHealthAssessmentFormService healthAssessmentFormService,
            IDoctorRecommendationService doctorRecommendationService)
        {
            _healthCampRepository = healthCampRepository;
            _intakeFormResponseRepository = intakeFormResponseRepository;
            _healthAssessmentFormService = healthAssessmentFormService;
            _doctorRecommendationService = doctorRecommendationService;
        }

        public async Task<CampData> FetchDataAsync(Guid campId, CancellationToken ct)
        {
            var camp = await _healthCampRepository.GetByIdAsync(campId)
                ?? throw new NotFoundException($"Health camp with ID {campId} not found.");

            var organizationName = await GetOrganizationNameAsync(camp, ct);

            var entityResponses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(campId, ct);
            if (!entityResponses.Any())
            {
                throw new NotFoundException("No intake form responses found for this camp.");
            }

            var campParticipants = await _healthCampRepository.GetParticipantsAsync(campId, null, null, ct);
            var campParticipantUserIds = campParticipants.Select(cp => cp.UserId).ToHashSet();

            var filteredEntityResponses = entityResponses
                .Where(r => r.Patient?.User != null && campParticipantUserIds.Contains(r.Patient.UserId))
                .ToList();

            if (!filteredEntityResponses.Any())
            {
                throw new NotFoundException("No intake form responses found for participants in this camp.");
            }

            var patientIds = filteredEntityResponses.Select(r => r.PatientId).Distinct().ToList();

            var allDtoResponses = new List<IntakeFormResponseDetailDto>();
            foreach (var patientId in patientIds)
            {
                var patientDtoResponses = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, campId, ct);
                allDtoResponses.AddRange(patientDtoResponses);
            }

            var healthAssessmentLookup = new Dictionary<Guid, List<HealthAssessmentResponseDto>>();
            foreach (var patientId in patientIds)
            {
                var patientHealthResponses = await _healthAssessmentFormService.GetPatientAssessmentResponsesAsync(patientId, campId, ct);
                healthAssessmentLookup[patientId] = patientHealthResponses;
            }

            // Fetch doctor recommendations by health camp
            var doctorRecommendations = await _doctorRecommendationService.GetByHealthCampAsync(campId, ct);

            return new CampData
            {
                Camp = camp,
                OrganizationName = organizationName,
                EntityResponses = filteredEntityResponses,
                DtoResponses = allDtoResponses,
                HealthAssessmentResponses = healthAssessmentLookup,
                DoctorRecommendations = doctorRecommendations
            };
        }

        private async Task<string> GetOrganizationNameAsync(HealthCamp camp, CancellationToken ct)
        {
            if (camp.Organization?.BusinessName != null)
            {
                return camp.Organization.BusinessName;
            }

            try
            {
                var campDetails = await _healthCampRepository.GetCampDetailsByIdAsync(camp.Id);
                if (campDetails?.ClientName != null)
                {
                    return campDetails.ClientName;
                }
            }
            catch
            {
                // Fallback if details fail
            }

            var participants = await _healthCampRepository.GetParticipantsAsync(camp.Id, null, null, ct);
            return participants.FirstOrDefault()?.HealthCamp?.Organization?.BusinessName ?? "Unknown_Organization";
        }
    }

    public class CampData
    {
        public HealthCamp Camp { get; set; } = null!;
        public string OrganizationName { get; set; } = string.Empty;
        public List<IntakeFormResponse> EntityResponses { get; set; } = [];
        public List<IntakeFormResponseDetailDto> DtoResponses { get; set; } = [];
        public Dictionary<Guid, List<HealthAssessmentResponseDto>> HealthAssessmentResponses { get; set; } = [];
        public IReadOnlyList<DoctorRecommendationResponseDto> DoctorRecommendations { get; set; } = [];
    }
}
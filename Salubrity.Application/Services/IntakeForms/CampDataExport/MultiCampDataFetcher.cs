using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthAssessments;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Application.Interfaces.Services.HealthAssessments;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.IntakeForms;
using Microsoft.Extensions.Logging;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class MultiCampDataFetcher
    {
        private readonly IHealthCampRepository _healthCampRepository;
        private readonly IIntakeFormResponseRepository _intakeFormResponseRepository;
        private readonly IHealthAssessmentFormService _healthAssessmentFormService;
        private readonly IDoctorRecommendationService _doctorRecommendationService;
        private readonly ILogger<MultiCampDataFetcher> _logger;

        public MultiCampDataFetcher(
            IHealthCampRepository healthCampRepository,
            IIntakeFormResponseRepository intakeFormResponseRepository,
            IHealthAssessmentFormService healthAssessmentFormService,
            IDoctorRecommendationService doctorRecommendationService,
            ILogger<MultiCampDataFetcher> logger)
        {
            _healthCampRepository = healthCampRepository;
            _intakeFormResponseRepository = intakeFormResponseRepository;
            _healthAssessmentFormService = healthAssessmentFormService;
            _doctorRecommendationService = doctorRecommendationService;
            _logger = logger;
        }

        public async Task<MultiCampData> FetchAllCampsDataAsync(CancellationToken ct)
        {
            _logger.LogInformation("Starting to fetch data for all camps");

            // Fetch all camps as DTOs
            var allCampsDto = await _healthCampRepository.GetAllAsync();
            if (!allCampsDto.Any())
            {
                _logger.LogWarning("No health camps found in the system");
                return new MultiCampData { CampDataList = new List<CampDataWithInfo>() };
            }

            _logger.LogInformation("Found {CampCount} camps to process", allCampsDto.Count);

            var campDataList = new List<CampDataWithInfo>();

            // Process camps SEQUENTIALLY to avoid DbContext concurrency issues
            var processedCount = 0;
            foreach (var campDto in allCampsDto)
            {
                try
                {
                    processedCount++;
                    _logger.LogInformation("Processing camp {Current}/{Total}: {CampName} ({CampId})",
                        processedCount, allCampsDto.Count, campDto.ClientName, campDto.Id);

                    // Fetch the full HealthCamp entity for this camp
                    var camp = await _healthCampRepository.GetByIdAsync(campDto.Id);
                    if (camp == null)
                    {
                        _logger.LogWarning("Camp {CampId} not found when fetching full entity", campDto.Id);
                        continue;
                    }

                    var campData = await FetchSingleCampDataAsync(camp, campDto.ClientName, ct);
                    if (campData != null)
                    {
                        campDataList.Add(campData);
                        _logger.LogInformation("Successfully fetched data for camp: {CampName} with {ParticipantCount} participants",
                            campData.CampName, campData.EntityResponses.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch data for camp {CampId} ({CampName}). Skipping.",
                        campDto.Id, campDto.ClientName);
                }
            }

            _logger.LogInformation("Successfully fetched data for {SuccessCount} out of {TotalCount} camps",
                campDataList.Count, allCampsDto.Count);

            return new MultiCampData { CampDataList = campDataList };
        }

        private async Task<CampDataWithInfo?> FetchSingleCampDataAsync(HealthCamp camp, string organizationName, CancellationToken ct)
        {
            // Fetch all responses for this camp
            var entityResponses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(camp.Id, ct);

            if (!entityResponses.Any())
            {
                _logger.LogDebug("No intake form responses found for camp {CampId}", camp.Id);
                return null;
            }

            // Get camp participants to filter responses
            var campParticipants = await _healthCampRepository.GetParticipantsAsync(camp.Id, null, null, ct);
            var campParticipantUserIds = campParticipants.Select(cp => cp.UserId).ToHashSet();

            var filteredEntityResponses = entityResponses
                .Where(r => r.Patient?.User != null && campParticipantUserIds.Contains(r.Patient.UserId))
                .ToList();

            if (!filteredEntityResponses.Any())
            {
                _logger.LogDebug("No valid participant responses found for camp {CampId}", camp.Id);
                return null;
            }

            var patientIds = filteredEntityResponses.Select(r => r.PatientId).Distinct().ToList();

            // Fetch DTO responses for all patients
            var allDtoResponses = new List<IntakeFormResponseDetailDto>();
            foreach (var patientId in patientIds)
            {
                var patientDtoResponses = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, camp.Id, ct);
                allDtoResponses.AddRange(patientDtoResponses);
            }

            // Fetch health assessments
            var healthAssessmentLookup = new Dictionary<Guid, List<HealthAssessmentResponseDto>>();
            foreach (var patientId in patientIds)
            {
                var responses = await _healthAssessmentFormService.GetPatientAssessmentResponsesAsync(patientId, camp.Id, ct);
                healthAssessmentLookup[patientId] = responses;
            }

            // Fetch doctor recommendations for this camp
            var doctorRecommendations = await _doctorRecommendationService.GetByHealthCampAsync(camp.Id, ct);

            return new CampDataWithInfo
            {
                Camp = camp,
                CampDate = camp.StartDate,
                OrganizationName = organizationName,
                EntityResponses = filteredEntityResponses,
                DtoResponses = allDtoResponses,
                HealthAssessmentResponses = healthAssessmentLookup,
                DoctorRecommendations = doctorRecommendations
            };
        }
    }

    public class MultiCampData
    {
        public List<CampDataWithInfo> CampDataList { get; set; } = [];
    }

    public class CampDataWithInfo
    {
        public HealthCamp Camp { get; set; } = null!;
        public DateTime CampDate { get; set; }
        public string CampName => Camp.Name;
        public string OrganizationName { get; set; } = string.Empty;
        public List<IntakeFormResponse> EntityResponses { get; set; } = [];
        public List<IntakeFormResponseDetailDto> DtoResponses { get; set; } = [];
        public Dictionary<Guid, List<HealthAssessmentResponseDto>> HealthAssessmentResponses { get; set; } = [];
        public IReadOnlyList<DoctorRecommendationResponseDto> DoctorRecommendations { get; set; } = [];
    }
}
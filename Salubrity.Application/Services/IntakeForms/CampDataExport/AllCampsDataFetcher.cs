using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthAssessments;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Application.Interfaces.Services.HealthAssessments;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class AllCampsDataFetcher
    {
        private readonly IHealthCampRepository _healthCampRepository;
        private readonly IIntakeFormResponseRepository _intakeFormResponseRepository;
        private readonly IHealthAssessmentFormService _healthAssessmentFormService;
        private readonly IDoctorRecommendationService _doctorRecommendationService;

        public AllCampsDataFetcher(
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

        public async Task<AllCampsData> FetchDataAsync(CancellationToken ct)
        {
            // Get all health camps
            var allCamps = await _healthCampRepository.GetAllAsync();
            if (!allCamps.Any())
            {
                return new AllCampsData { CampDataList = [] };
            }

            var campDataList = new List<CampDataWithInfo>();

            // Process camps in smaller batches to avoid memory issues
            const int batchSize = 10; // Process 10 camps at a time
            var campBatches = allCamps.OrderBy(c => c.StartDate)
                .Select((camp, index) => new { camp, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.camp).ToList())
                .ToList();

            foreach (var batch in campBatches)
            {
                foreach (var campSummary in batch)
                {
                    try
                    {
                        // Get detailed camp information
                        var camp = await _healthCampRepository.GetByIdAsync(campSummary.Id);
                        if (camp == null) continue;

                        var organizationName = await GetOrganizationNameAsync(camp, ct);

                        // Get responses for this camp
                        var entityResponses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(camp.Id, ct);
                        if (!entityResponses.Any()) continue;

                        // Get camp participants to filter responses
                        var campParticipants = await _healthCampRepository.GetParticipantsAsync(camp.Id, null, null, ct);
                        var campParticipantUserIds = campParticipants.Select(cp => cp.UserId).ToHashSet();

                        var filteredEntityResponses = entityResponses
                            .Where(r => r.Patient?.User != null && campParticipantUserIds.Contains(r.Patient.UserId))
                            .ToList();

                        if (!filteredEntityResponses.Any()) continue;

                        var patientIds = filteredEntityResponses.Select(r => r.PatientId).Distinct().ToList();

                        // Get DTO responses for patients in this camp (sequential processing)
                        var allDtoResponses = new List<IntakeFormResponseDetailDto>();
                        foreach (var patientId in patientIds)
                        {
                            var patientDtoResponses = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, camp.Id, ct);
                            allDtoResponses.AddRange(patientDtoResponses);
                        }

                        // Get health assessment responses (sequential processing)
                        var healthAssessmentLookup = new Dictionary<Guid, List<HealthAssessmentResponseDto>>();
                        foreach (var patientId in patientIds)
                        {
                            var patientHealthResponses = await _healthAssessmentFormService.GetPatientAssessmentResponsesAsync(patientId, camp.Id, ct);
                            healthAssessmentLookup[patientId] = patientHealthResponses;
                        }

                        // Get doctor recommendations
                        var doctorRecommendations = await _doctorRecommendationService.GetByHealthCampAsync(camp.Id, ct);

                        campDataList.Add(new CampDataWithInfo
                        {
                            Camp = camp,
                            OrganizationName = organizationName,
                            CampDate = camp.StartDate,
                            EntityResponses = filteredEntityResponses,
                            DtoResponses = allDtoResponses,
                            HealthAssessmentResponses = healthAssessmentLookup,
                            DoctorRecommendations = doctorRecommendations
                        });

                        // Add a small delay to prevent overwhelming the database
                        await Task.Delay(10, ct);
                    }
                    catch (Exception)
                    {
                        // Log error and continue with next camp
                        continue;
                    }
                }

                // Short pause between batches to allow other operations
                await Task.Delay(50, ct);
            }

            return new AllCampsData
            {
                CampDataList = campDataList.OrderBy(c => c.CampDate).ToList()
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

    public class AllCampsData
    {
        public List<CampDataWithInfo> CampDataList { get; set; } = [];
    }

    public class CampDataWithInfo
    {
        public HealthCamp Camp { get; set; } = null!;
        public string OrganizationName { get; set; } = string.Empty;
        public DateTime CampDate { get; set; }
        public List<IntakeFormResponse> EntityResponses { get; set; } = [];
        public List<IntakeFormResponseDetailDto> DtoResponses { get; set; } = [];
        public Dictionary<Guid, List<HealthAssessmentResponseDto>> HealthAssessmentResponses { get; set; } = [];
        public IReadOnlyList<DoctorRecommendationResponseDto> DoctorRecommendations { get; set; } = [];
    }
}
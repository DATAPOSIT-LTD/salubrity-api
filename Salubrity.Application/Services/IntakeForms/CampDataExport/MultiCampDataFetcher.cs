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
using System.Diagnostics;

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
            var overallStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("====== STARTING OPTIMIZED BATCH DATA FETCH ======");

            // Step 1: Fetch all camps WITH full entity details in ONE query
            var sw = Stopwatch.StartNew();
            var allCamps = await _healthCampRepository.GetAllWithDetailsAsync(ct); // New method needed
            sw.Stop();
            _logger.LogInformation("⏱️ Fetched all camps with details: {ElapsedMs}ms ({CampCount} camps)",
                sw.ElapsedMilliseconds, allCamps.Count);

            if (!allCamps.Any())
            {
                _logger.LogWarning("No health camps found");
                return new MultiCampData { CampDataList = new List<CampDataWithInfo>() };
            }

            var campIds = allCamps.Select(c => c.Id).ToList();
            var campById = allCamps.ToDictionary(c => c.Id);

            // Step 2: BATCH fetch ALL participants for ALL camps in ONE query
            sw = Stopwatch.StartNew();
            var participantsByCampId = await _healthCampRepository.GetParticipantsForMultipleCampsAsync(campIds, ct);
            sw.Stop();
            _logger.LogInformation("⏱️ Batch fetched participants: {ElapsedMs}ms ({Count} participants across {CampCount} camps)",
                sw.ElapsedMilliseconds, participantsByCampId.Values.Sum(p => p.Count), participantsByCampId.Count);

            // Step 3: BATCH fetch ALL intake responses for ALL camps in ONE query
            sw = Stopwatch.StartNew();
            var responseByCampId = await _intakeFormResponseRepository.GetResponsesForMultipleCampsAsync(campIds, ct);
            sw.Stop();
            _logger.LogInformation("⏱️ Batch fetched intake responses: {ElapsedMs}ms ({Count} responses)",
                sw.ElapsedMilliseconds, responseByCampId.Values.Sum(list => list.Count));

            // Step 4: Get all unique patient IDs
            var allPatientIds = responseByCampId.Values
                .SelectMany(responses => responses.Select(r => r.PatientId))
                .Distinct()
                .ToList();

            // Step 5: BATCH fetch DTO responses
            sw = Stopwatch.StartNew();
            var dtoResponsesByPatient = await _intakeFormResponseRepository.GetDtoResponsesForMultipleCampsAsync(campIds, allPatientIds, ct);
            sw.Stop();
            _logger.LogInformation("⏱️ Batch fetched DTO responses: {ElapsedMs}ms ({Count} responses)",
                sw.ElapsedMilliseconds, dtoResponsesByPatient.Values.Sum(list => list.Count));

            // Step 6: Build camp data (now WITHOUT per-camp queries)
            sw = Stopwatch.StartNew();
            var campDataList = new List<CampDataWithInfo>();

            foreach (var campId in campIds)
            {
                if (!campById.TryGetValue(campId, out var camp)) continue;
                if (!responseByCampId.TryGetValue(campId, out var entityResponses) || !entityResponses.Any()) continue;
                if (!participantsByCampId.TryGetValue(campId, out var campParticipants)) continue;

                var campParticipantUserIds = campParticipants.Select(cp => cp.UserId).ToHashSet();

                var filteredEntityResponses = entityResponses
                    .Where(r => r.Patient?.User != null && campParticipantUserIds.Contains(r.Patient.UserId))
                    .ToList();

                if (!filteredEntityResponses.Any()) continue;

                var campPatientIds = filteredEntityResponses.Select(r => r.PatientId).Distinct().ToList();

                var campDtoResponses = campPatientIds
                    .Where(pid => dtoResponsesByPatient.ContainsKey(pid))
                    .SelectMany(pid => dtoResponsesByPatient[pid])
                    .ToList();

                var orgName = allCamps.FirstOrDefault(c => c.Id == campId)?.Organization?.BusinessName ?? "";

                campDataList.Add(new CampDataWithInfo
                {
                    Camp = camp,
                    CampDate = camp.StartDate,
                    OrganizationName = orgName,
                    EntityResponses = filteredEntityResponses,
                    DtoResponses = campDtoResponses,
                    HealthAssessmentResponses = new Dictionary<Guid, List<HealthAssessmentResponseDto>>(),
                    DoctorRecommendations = new List<DoctorRecommendationResponseDto>()
                });
            }

            sw.Stop();
            _logger.LogInformation("⏱️ Built camp data: {ElapsedMs}ms ({CampCount} camps)",
                sw.ElapsedMilliseconds, campDataList.Count);

            overallStopwatch.Stop();
            _logger.LogInformation("====== DATA FETCH COMPLETED ======");
            _logger.LogInformation("⏱️ Total time: {TotalSeconds:F2}s", overallStopwatch.Elapsed.TotalSeconds);

            return new MultiCampData { CampDataList = campDataList };
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
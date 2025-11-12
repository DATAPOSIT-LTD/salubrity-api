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
            _logger.LogInformation("====== STARTING BATCHED DATA FETCH ======");

            var sw = Stopwatch.StartNew();
            var allCamps = await _healthCampRepository.GetAllWithDetailsAsync(ct);
            sw.Stop();
            _logger.LogInformation("⏱️ Fetched camps: {ElapsedMs}ms ({Count} camps)",
                sw.ElapsedMilliseconds, allCamps.Count);

            if (!allCamps.Any())
            {
                return new MultiCampData { CampDataList = new List<CampDataWithInfo>() };
            }

            var campIds = allCamps.Select(c => c.Id).ToList();
            var campById = allCamps.ToDictionary(c => c.Id);

            // Fetch participants
            sw = Stopwatch.StartNew();
            var participantsByCampId = await _healthCampRepository.GetParticipantsForMultipleCampsAsync(campIds, ct);
            sw.Stop();
            _logger.LogInformation("⏱️ Fetched participants: {ElapsedMs}ms ({Count} total)",
                sw.ElapsedMilliseconds, participantsByCampId.Values.Sum(p => p.Count));

            // Process in batches of 15 camps to avoid timeout
            const int batchSize = 15;
            var campBatches = campIds.Chunk(batchSize).ToList();
            var allResponsesByCamp = new Dictionary<Guid, List<IntakeFormResponse>>();
            var allDtoResponsesByPatient = new Dictionary<Guid, List<IntakeFormResponseDetailDto>>();

            _logger.LogInformation("📦 Processing {BatchCount} batches of max {BatchSize} camps",
                campBatches.Count, batchSize);

            for (int i = 0; i < campBatches.Count; i++)
            {
                var batchCampIds = campBatches[i].ToList();

                sw = Stopwatch.StartNew();

                try
                {
                    // Fetch responses for this batch
                    var batchResponses = await _intakeFormResponseRepository
                        .GetResponsesForMultipleCampsAsync(batchCampIds, ct);

                    // Get patient IDs
                    var batchPatientIds = batchResponses.Values
                        .SelectMany(responses => responses.Select(r => r.PatientId))
                        .Distinct()
                        .ToList();

                    // Fetch DTO responses
                    var batchDtoResponses = await _intakeFormResponseRepository
                        .GetDtoResponsesForMultipleCampsAsync(batchCampIds, batchPatientIds, ct);

                    sw.Stop();

                    // Merge results
                    foreach (var kvp in batchResponses)
                    {
                        allResponsesByCamp[kvp.Key] = kvp.Value;
                    }

                    foreach (var kvp in batchDtoResponses)
                    {
                        if (!allDtoResponsesByPatient.ContainsKey(kvp.Key))
                        {
                            allDtoResponsesByPatient[kvp.Key] = new List<IntakeFormResponseDetailDto>();
                        }
                        allDtoResponsesByPatient[kvp.Key].AddRange(kvp.Value);
                    }

                    _logger.LogInformation("  ✅ Batch {Current}/{Total}: {ElapsedMs}ms ({CampCount} camps, {ResponseCount} responses)",
                        i + 1, campBatches.Count, sw.ElapsedMilliseconds,
                        batchCampIds.Count, batchResponses.Values.Sum(list => list.Count));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Batch {Current}/{Total} failed", i + 1, campBatches.Count);
                    throw;
                }
            }

            // Build camp data
            sw = Stopwatch.StartNew();
            var campDataList = new List<CampDataWithInfo>();

            foreach (var campId in campIds)
            {
                if (!campById.TryGetValue(campId, out var camp)) continue;
                if (!allResponsesByCamp.TryGetValue(campId, out var entityResponses) || !entityResponses.Any()) continue;
                if (!participantsByCampId.TryGetValue(campId, out var campParticipants)) continue;

                var campParticipantUserIds = campParticipants.Select(cp => cp.UserId).ToHashSet();

                var filteredEntityResponses = entityResponses
                    .Where(r => r.Patient?.User != null && campParticipantUserIds.Contains(r.Patient.UserId))
                    .ToList();

                if (!filteredEntityResponses.Any()) continue;

                var campPatientIds = filteredEntityResponses.Select(r => r.PatientId).Distinct().ToList();

                var campDtoResponses = campPatientIds
                    .Where(pid => allDtoResponsesByPatient.ContainsKey(pid))
                    .SelectMany(pid => allDtoResponsesByPatient[pid])
                    .ToList();

                campDataList.Add(new CampDataWithInfo
                {
                    Camp = camp,
                    CampDate = camp.StartDate,
                    OrganizationName = camp.Organization?.BusinessName ?? "",
                    EntityResponses = filteredEntityResponses,
                    DtoResponses = campDtoResponses,
                    HealthAssessmentResponses = new Dictionary<Guid, List<HealthAssessmentResponseDto>>(),
                    DoctorRecommendations = new List<DoctorRecommendationResponseDto>()
                });
            }

            sw.Stop();
            _logger.LogInformation("⏱️ Built camp data: {ElapsedMs}ms ({Count} camps)",
                sw.ElapsedMilliseconds, campDataList.Count);

            overallStopwatch.Stop();
            _logger.LogInformation("====== FETCH COMPLETED: {TotalSeconds:F2}s ======",
                overallStopwatch.Elapsed.TotalSeconds);

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
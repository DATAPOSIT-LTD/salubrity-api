#nullable enable

using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Logging;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Application.Interfaces.Services.HealthAssessments;
using Salubrity.Application.Interfaces.Services.Notifications;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Application.Services.IntakeForms.CampDataExport;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Shared.Exceptions;
using System.Diagnostics;

namespace Salubrity.Application.Services.Forms;

public sealed class IntakeFormResponseService : IIntakeFormResponseService
{
    private readonly IIntakeFormResponseRepository _intakeFormResponseRepository;
    private readonly IHealthCampParticipantRepository _participantRepository;
    private readonly ICampQueueRepository _stationCheckInRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceCategoryRepository _serviceCategoryRepository;
    private readonly IServiceSubcategoryRepository _serviceSubcategoryRepository;
    private readonly IHealthCampServiceAssignmentRepository _assignmentRepository;
    private readonly ILogger<IntakeFormResponseService> _logger;
    private readonly IHealthCampService _campService;
    private readonly IIntakeFormRepository _intakeFormRepository;
    private readonly IHealthCampRepository _healthCampRepository;
    private readonly IHealthAssessmentFormService _healthAssessmentFormService;
    private readonly IHealthCampParticipantServiceStatusRepository _participantServiceStatusRepository;
    private readonly IDoctorRecommendationService _doctorRecommendationService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly INotificationService _notificationService;
    private readonly IRoleRepository _roleRepository;

    // --------------------------------------------------
    // Triage alert evaluation
    // --------------------------------------------------
    // Note: do NOT hardcode field IDs. We resolve field metadata from the submitted IntakeFormVersion
    // and evaluate based on FieldType / Label / MinValue / MaxValue to remain compatible with multiple versions.


    public IntakeFormResponseService(
        IIntakeFormResponseRepository intakeFormResponseRepository,
        IHealthCampParticipantRepository participantRepository,
        ICampQueueRepository campQueueRepository,
        IHealthCampServiceAssignmentRepository assignmentRepository,
        IServiceRepository serviceRepository,
        IServiceCategoryRepository serviceCategoryRepository,
        IServiceSubcategoryRepository serviceSubcategoryRepository,
        ILogger<IntakeFormResponseService> logger,
        INotificationService notificationService,
        IHealthCampService campService,
        IIntakeFormRepository intakeFormRepository,
        IHealthCampRepository healthCampRepository,
        IHealthAssessmentFormService healthAssessmentFormService,

        IHealthCampParticipantServiceStatusRepository participantServiceStatusRepository,
        IDoctorRecommendationService doctorRecommendationService,
        ILoggerFactory loggerFactory,
        IRoleRepository roleRepository
    )
    {
        _intakeFormResponseRepository = intakeFormResponseRepository;
        _participantRepository = participantRepository;
        _stationCheckInRepository = campQueueRepository;
        _assignmentRepository = assignmentRepository;
        _serviceRepository = serviceRepository;
        _serviceCategoryRepository = serviceCategoryRepository;
        _serviceSubcategoryRepository = serviceSubcategoryRepository;
        _logger = logger;
        _notificationService = notificationService;
        _campService = campService;
        _intakeFormRepository = intakeFormRepository;
        _healthCampRepository = healthCampRepository;
        _healthAssessmentFormService = healthAssessmentFormService;
        _participantServiceStatusRepository = participantServiceStatusRepository;
        _doctorRecommendationService = doctorRecommendationService;
        _loggerFactory = loggerFactory;
        _roleRepository = roleRepository;
    }

    public async Task<Guid> SubmitResponseAsync(CreateIntakeFormResponseDto dto, Guid submittedByUserId, CancellationToken ct = default)
    {
        _logger.LogInformation("Submitting intake form for participant: {ParticipantId}, SubmittedBy: {UserId}", dto.PatientId, submittedByUserId);

        var versionExists = await _intakeFormResponseRepository.IntakeFormVersionExistsAsync(dto.IntakeFormVersionId, ct);
        if (!versionExists)
            throw new NotFoundException("Form version not found.");

        var validFieldIds = await _intakeFormResponseRepository.GetFieldIdsForVersionAsync(dto.IntakeFormVersionId, ct);
        var invalidField = dto.FieldResponses.FirstOrDefault(f => !validFieldIds.Contains(f.FieldId));
        if (invalidField is not null)
            throw new ValidationException([$"Field {invalidField.FieldId} does not belong to form version {dto.IntakeFormVersionId}."]);

        var patientId = await _participantRepository.GetPatientIdByParticipantIdAsync(dto.ParticipantId, ct);
        if (patientId is null)
            throw new NotFoundException($"Patient not found for participant {dto.ParticipantId}.");

        Guid submittedServiceId;
        PackageItemType submittedServiceType;
        Guid resolvedServiceId;

        // ---- PATH A: Assignment provided (already correct) ----
        if (dto.HealthCampServiceAssignmentId.HasValue)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(dto.HealthCampServiceAssignmentId.Value, ct)
                ?? throw new NotFoundException($"Assignment not found: {dto.HealthCampServiceAssignmentId}");

            submittedServiceId = assignment.AssignmentId;
            submittedServiceType = assignment.AssignmentType;

            _logger.LogInformation("Assignment found: Type={Type}, Id={Id}", submittedServiceType, submittedServiceId);

            resolvedServiceId = assignment.AssignmentType switch
            {
                PackageItemType.Service => assignment.AssignmentId,

                PackageItemType.ServiceCategory =>
                    (await _serviceCategoryRepository.GetByIdAsync(assignment.AssignmentId))?.ServiceId
                    ?? throw new ValidationException(["ServiceCategory is not linked to a root Service."]),

                PackageItemType.ServiceSubcategory =>
                    (await _serviceSubcategoryRepository.GetByIdAsync(assignment.AssignmentId))?.ServiceCategory?.ServiceId
                    ?? throw new ValidationException(["Subcategory is not linked to a root Service via its category."]),

                _ => throw new ValidationException([$"Unsupported assignment type: {assignment.AssignmentType}"])
            };
        }
        // ---- PATH B: Only ServiceId provided by client (MUST resolve & detect type) ----
        else if (dto.ServiceId.HasValue)
        {
            var incomingRefId = dto.ServiceId.Value;
            _logger.LogInformation("Incoming reference Id (from client ServiceId field): {RefId}", incomingRefId);

            // Detect what the incomingRefId actually is and resolve to top-level ServiceId
            if (await _serviceRepository.ExistsByIdAsync(incomingRefId, ct))
            {
                submittedServiceType = PackageItemType.Service;
                submittedServiceId = incomingRefId;
                resolvedServiceId = incomingRefId;
                _logger.LogInformation("Detected type=Service. Using ServiceId as resolved: {ServiceId}", resolvedServiceId);
            }
            else
            {
                // Try category
                var category = await _serviceCategoryRepository.GetByIdAsync(incomingRefId);
                if (category is not null)
                {
                    submittedServiceType = PackageItemType.ServiceCategory;
                    submittedServiceId = incomingRefId;
                    resolvedServiceId = category.ServiceId;
                    _logger.LogInformation("Detected type=ServiceCategory. Resolved root ServiceId: {ServiceId}", resolvedServiceId);
                }
                else
                {
                    // Try subcategory
                    var subcategory = await _serviceSubcategoryRepository.GetByIdAsync(incomingRefId);
                    if (subcategory is not null)
                    {
                        submittedServiceType = PackageItemType.ServiceSubcategory;
                        submittedServiceId = incomingRefId;
                        var parentServiceId = subcategory.ServiceCategory?.ServiceId;
                        if (parentServiceId == null)
                            throw new ValidationException(["Subcategory is not linked to a root Service via its category."]);
                        resolvedServiceId = parentServiceId.Value;
                        _logger.LogInformation("Detected type=ServiceSubcategory. Resolved root ServiceId: {ServiceId}", resolvedServiceId);
                    }
                    else
                    {
                        _logger.LogError("Incoming reference Id does not match Service/Category/Subcategory: {RefId}", incomingRefId);
                        throw new ValidationException([$"Invalid ServiceId: {incomingRefId} is not a Service, ServiceCategory, or ServiceSubcategory."]);
                    }
                }
            }
        }
        else
        {
            throw new ValidationException(["Either HealthCampServiceAssignmentId or ServiceId must be provided."]);
        }

        _logger.LogInformation("🔗 Final Resolved ServiceId (must exist in Services): {ResolvedServiceId}", resolvedServiceId);

        var serviceExists = await _serviceRepository.ExistsByIdAsync(resolvedServiceId, ct);
        if (!serviceExists)
        {
            _logger.LogError("❌ Resolved ServiceId does NOT exist in Services table: {ResolvedServiceId}", resolvedServiceId);
            throw new ValidationException([$"ResolvedServiceId does not exist in Services table: {resolvedServiceId}"]);
        }

        var statusId = dto.ResponseStatusId
            ?? await _intakeFormResponseRepository.GetStatusIdByNameAsync("Submitted", ct);

        var responseId = Guid.NewGuid();
        var response = new IntakeFormResponse
        {
            Id = responseId,
            IntakeFormVersionId = dto.IntakeFormVersionId,
            SubmittedByUserId = submittedByUserId,
            PatientId = patientId.Value,
            SubmittedServiceId = submittedServiceId,     // raw thing client sent (service/category/subcategory)
            SubmittedServiceType = submittedServiceType, // detected type of that raw thing
            ResolvedServiceId = resolvedServiceId,       // top-level ServiceId (FK-safe)
            ResponseStatusId = statusId,
            HealthCampId = dto.HealthCampId,
            FieldResponses = dto.FieldResponses.Select(f => new IntakeFormFieldResponse
            {
                Id = Guid.NewGuid(),
                ResponseId = responseId,
                FieldId = f.FieldId,
                Value = f.Value
            }).ToList()
        };

        _logger.LogInformation("📥 Saving IntakeFormResponse: Id={Id}, Version={VersionId}, Fields={FieldCount}",
            response.Id, response.IntakeFormVersionId, response.FieldResponses.Count);

        await _intakeFormResponseRepository.AddAsync(response, ct);

        // --- Triage alerts (doctor + concierge only) ---
        await TryGenerateTriageAlertsAsync(response, dto, resolvedServiceId, ct);

        // --- Check-in flow ---
        HealthCampStationCheckIn? checkIn = null;

        if (dto.StationCheckInId is Guid explicitCheckInId)
        {
            checkIn = await _stationCheckInRepository.GetByIdAsync(explicitCheckInId, ct)
                ?? throw new NotFoundException($"Station check-in not found: {explicitCheckInId}");
        }
        else
        {
            checkIn = await _stationCheckInRepository.GetActiveForParticipantAsync(
                dto.ParticipantId,
                dto.HealthCampServiceAssignmentId,
                ct);
        }

        if (checkIn is not null)
        {
            if (checkIn.Status is CampQueueStatus.Canceled or CampQueueStatus.Completed)
                throw new ValidationException([$"Cannot complete a check-in in '{checkIn.Status}' status."]);

            var now = DateTimeOffset.UtcNow;
            checkIn.Status = CampQueueStatus.Completed;
            checkIn.FinishedAt = now;
            checkIn.StartedAt ??= now;

            // _

            _logger.LogInformation("Marking station check-in as completed: CheckInId={CheckInId}, FinishedAt={FinishedAt}", checkIn.Id, now);
            await _stationCheckInRepository.UpdateAsync(checkIn, ct);
        }



        // --- Mark participant as served for this specific service station ---
        // Run when client sends either HealthCampServiceAssignmentId (assignment Id) or ServiceId (we resolve to assignment)
        Guid? resolvedAssignmentId = null;
        if (dto.HealthCampServiceAssignmentId.HasValue)
        {
            var assignmentById = await _assignmentRepository.GetByIdAsync(dto.HealthCampServiceAssignmentId.Value, ct);
            if (assignmentById != null)
                resolvedAssignmentId = assignmentById.Id;
        }
        else if (dto.ServiceId.HasValue)
        {
            var participant = await _participantRepository
                .GetParticipantWithBillingStatusByIdAsync(dto.ParticipantId, ct);
            if (participant == null)
                throw new ValidationException([$"Participant {dto.ParticipantId} not found."]);
            var campId = participant.HealthCampId;
            var assignment = await _assignmentRepository.FirstOrDefaultAsync(
                a => a.AssignmentId == dto.ServiceId.Value &&
                     a.HealthCampId == campId &&
                     !a.IsDeleted,
                ct);
            if (assignment == null)
                throw new ValidationException([$"No HealthCampServiceAssignment found for ServiceId {dto.ServiceId} in camp {campId}"]);
            resolvedAssignmentId = assignment.Id;
        }

        if (resolvedAssignmentId.HasValue)
        {
            var participantService = await _participantServiceStatusRepository
                .GetByParticipantAndAssignmentAsync(dto.ParticipantId, resolvedAssignmentId.Value, ct);

            if (participantService == null)
            {
                participantService = new HealthCampParticipantServiceStatus
                {
                    Id = Guid.NewGuid(),
                    ParticipantId = dto.ParticipantId,
                    ServiceAssignmentId = resolvedAssignmentId.Value,
                    SubcontractorId = submittedByUserId,
                    ServedAt = DateTime.UtcNow
                };

                await _participantServiceStatusRepository.AddAsync(participantService, ct);
                _logger.LogInformation(
                    "Marked participant {ParticipantId} as served at assignment {AssignmentId}",
                    dto.ParticipantId,
                    resolvedAssignmentId.Value);
            }
            else if (participantService.ServedAt == null)
            {
                participantService.ServedAt = DateTime.UtcNow;
                await _participantServiceStatusRepository.UpdateAsync(participantService, ct);
                _logger.LogInformation("🔄 Updated service served timestamp for participant {ParticipantId}", dto.ParticipantId);
            }
        }



        _logger.LogInformation("Intake form submitted successfully. ResponseId={ResponseId}", responseId);
        return responseId;
    }

    // --------------------------------------------------
    // Triage alert helpers
    // --------------------------------------------------

    private async Task TryGenerateTriageAlertsAsync(
        IntakeFormResponse response,
        CreateIntakeFormResponseDto dto,
        Guid resolvedServiceId,
        CancellationToken ct)
    {
        // Only triage-like services
        var service = await _serviceRepository.GetByIdAsync(resolvedServiceId, ct);
        if (service == null ||
            !service.Name.Contains("Triage", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Load schema for this version so alerts work across multiple triage versions
        var version = await _intakeFormRepository.GetVersionWithFieldsAsync(dto.IntakeFormVersionId, ct);
        if (version == null)
            return;

        var fieldMeta = version.Sections
            .SelectMany(s => s.Fields)
            .GroupBy(f => f.Id)
            .ToDictionary(g => g.Key, g => g.First());

        // Map field values by fieldId from the incoming DTO
        var valueByFieldId = dto.FieldResponses
            .GroupBy(fr => fr.FieldId)
            .ToDictionary(g => g.Key, g => g.Last().Value?.Trim() ?? string.Empty);

        var issues = EvaluateTriageAbnormalities(valueByFieldId, fieldMeta);
        if (issues.Count == 0)
            return;

        // Build context
        string patientLabel = response.PatientId.ToString();
        try
        {
            var participant = await _participantRepository.GetParticipantWithBillingStatusByIdAsync(dto.ParticipantId, ct);
            if (participant?.User != null)
                patientLabel = participant.User.FullName ?? patientLabel;
        }
        catch
        {
            // best-effort only; do not fail submission on alert enrichment
        }

        string campName = response.HealthCampId?.ToString() ?? "Unknown camp";
        try
        {
            if (response.HealthCampId.HasValue)
            {
                var camp = await _healthCampRepository.GetByIdWithPackagesAsync(response.HealthCampId.Value, ct);
                if (camp != null)
                    campName = camp.Name ?? campName;
            }
        }
        catch
        {
            // ignore enrichment failures
        }

        var message =
            $"Triage alert for patient {patientLabel} at camp '{campName}': " +
            string.Join("; ", issues);

        // Notify Doctor and Concierge roles only
        var doctorRole = await SafeFindRoleByNameAsync("Doctor", ct);
        var conciergeRole = await SafeFindRoleByNameAsync("Concierge", ct);

        if (doctorRole != null)
        {
            await _notificationService.TriggerNotificationAsync(
                title: "Triage Alert",
                message: message,
                type: "TriageAlert",
                entityId: doctorRole.Id,
                entityType: "Role",
                ct: ct);
        }

        if (conciergeRole != null)
        {
            await _notificationService.TriggerNotificationAsync(
                title: "Triage Alert",
                message: message,
                type: "TriageAlert",
                entityId: conciergeRole.Id,
                entityType: "Role",
                ct: ct);
        }
    }

    private async Task<Domain.Entities.Rbac.Role?> SafeFindRoleByNameAsync(string roleName, CancellationToken ct)
    {
        try
        {
            return await _roleRepository.FindByNameAsync(roleName);
        }
        catch
        {
            return null;
        }
    }

    private static List<string> EvaluateTriageAbnormalities(
        IReadOnlyDictionary<Guid, string> values,
        IReadOnlyDictionary<Guid, IntakeFormField> fieldMeta)
    {
        var issues = new List<string>();

        foreach (var (fieldId, raw) in values)
        {
            if (!fieldMeta.TryGetValue(fieldId, out var field))
                continue;

            var fieldType = field.FieldType?.Trim().ToLowerInvariant();
            var label = field.Label?.Trim() ?? "Field";
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            // Blood pressure: "systolic/diastolic" stored as string (e.g. "110/90")
            if (fieldType == "blood-pressure")
            {
                if (!raw.Contains('/'))
                    continue;

                var parts = raw.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], out var systolic) ||
                    !int.TryParse(parts[1], out var diastolic))
                    continue;

                var prefix = label;

                if (systolic <= diastolic)
                    issues.Add($"{prefix} pattern unusual: systolic ({systolic}) ≤ diastolic ({diastolic})");

                // Default BP thresholds (can be externalized later)
                if (systolic > 140 || diastolic > 90)
                    issues.Add($"{prefix} high: {systolic}/{diastolic} mmHg");
                else if (systolic < 90 || diastolic < 60)
                    issues.Add($"{prefix} low: {systolic}/{diastolic} mmHg");

                continue;
            }

            // Numeric fields: use schema min/max if defined
            if (fieldType == "number")
            {
                if (!decimal.TryParse(raw, out var num))
                    continue;

                if (field.MinValue.HasValue && num < field.MinValue.Value)
                    issues.Add($"{label} below minimum ({field.MinValue.Value}): {raw}");
                else if (field.MaxValue.HasValue && num > field.MaxValue.Value)
                    issues.Add($"{label} above maximum ({field.MaxValue.Value}): {raw}");

                // Common clinical heuristics by label (work across versions)
                if (label.Contains("oxygen", StringComparison.OrdinalIgnoreCase) && num < 90)
                    issues.Add($"{label} low: {raw}%");

                if (label.Contains("temperature", StringComparison.OrdinalIgnoreCase) && (num < 35 || num > 38))
                    issues.Add($"{label} abnormal: {raw} °C");

                if (label.Contains("heart rate", StringComparison.OrdinalIgnoreCase) && (num < 50 || num > 120))
                    issues.Add($"{label} abnormal: {raw} bpm");

                if (label.Contains("bmi", StringComparison.OrdinalIgnoreCase) && (num < 18.5m || num > 30m))
                    issues.Add($"{label} abnormal: {raw}");
            }
        }

        return issues;
    }

    public async Task PatchResponseAsync(
         PatchIntakeFormResponseDto dto,
         Guid actingUserId,
         CancellationToken ct)
    {
        var response = await _intakeFormResponseRepository
            .GetWithFieldResponsesAsync(dto.ResponseId, ct);

        if (response == null)
            throw new NotFoundException(
                "IntakeFormResponse",
                dto.ResponseId.ToString());

        // ------------------------------------
        // Optional status update
        // ------------------------------------
        if (dto.ResponseStatusId.HasValue &&
            dto.ResponseStatusId.Value != response.ResponseStatusId)
        {
            response.ResponseStatusId = dto.ResponseStatusId.Value;
        }

        // ------------------------------------
        // Field UPSERT (domain-safe)
        // ------------------------------------
        foreach (var incoming in dto.FieldResponses)
        {
            var existing = response.FieldResponses
                .FirstOrDefault(fr => fr.FieldId == incoming.FieldId);

            if (existing != null)
            {
                existing.Value = incoming.Value;
                existing.UpdatedBy = actingUserId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                response.FieldResponses.Add(new IntakeFormFieldResponse
                {
                    ResponseId = response.Id,
                    FieldId = incoming.FieldId,
                    Value = incoming.Value,
                    CreatedBy = actingUserId
                });
            }
        }

        response.UpdatedBy = actingUserId;
        response.UpdatedAt = DateTime.UtcNow;

        await _intakeFormResponseRepository.SaveChangesAsync(ct);
    }

    public async Task<IntakeFormResponseDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _intakeFormResponseRepository.GetByIdAsync(id, ct);
        if (entity is null) return null;

        return new IntakeFormResponseDto
        {
            Id = entity.Id,
            IntakeFormId = entity.Version?.IntakeFormId ?? Guid.Empty,
            IntakeFormVersionId = entity.IntakeFormVersionId,
            SubmittedByUserId = entity.SubmittedByUserId,
            PatientId = entity.PatientId,
            SubmittedServiceId = entity.SubmittedServiceId,
            SubmittedServiceType = entity.SubmittedServiceType,
            ResolvedServiceId = entity.ResolvedServiceId,
            ResponseStatusId = entity.ResponseStatusId,
            HealthCampId = entity.HealthCampId ?? Guid.Empty,
            FieldResponses = entity.FieldResponses
                .OrderBy(f => f.CreatedAt)
                .Select(field => new IntakeFormFieldResponseDto
                {
                    Id = field.Id,
                    ResponseId = field.ResponseId,
                    FieldId = field.FieldId,
                    Value = field.Value
                }).ToList()
        };
    }

    public async Task<List<IntakeFormResponseDetailDto>> GetResponsesByPatientAndCampIdAsync(Guid patientId, Guid healthCampId, CancellationToken ct = default)
    {
        var responses = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, healthCampId, ct);

        if (!responses.Any())
            throw new NotFoundException("No responses found for this patient in this camp.");

        return responses;
    }

    // Download Findings Implementation

    public async Task<(
        byte[] ExcelData,
        string CampName,
        string OrganizationName,
        DateTime ExportTimestamp
    )> ExportCampDataToExcelAsync(
        Guid campId,
        Guid? branchId = null,
        CancellationToken ct = default)
    {
        var exportTimestamp = DateTime.Now.AddHours(3);

        // 1. Fetch Data
        var dataFetcher = new CampDataFetcher(
            _healthCampRepository,
            _intakeFormResponseRepository,
            _healthAssessmentFormService,
            _doctorRecommendationService
        );

        var campData = await dataFetcher.FetchDataAsync(campId, branchId, ct);

        // 2. Process Data
        var dataProcessor = new CampDataProcessor();
        var processedData = dataProcessor.Process(campData);

        // 3. Export Data
        var exporter = new CampDataExcelExporter();
        var excelData = exporter.Export(processedData);

        return (excelData, processedData.CampName, processedData.OrganizationName, exportTimestamp);
    }

    public async Task<(byte[] ExcelData, DateTime ExportTimestamp)> ExportAllCampsDataToExcelAsync(CancellationToken ct = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var exportTimestamp = DateTime.Now.AddHours(3);

        _logger.LogInformation("========================================");
        _logger.LogInformation("🚀 STARTING MULTI-CAMP EXPORT at {Timestamp}", exportTimestamp);
        _logger.LogInformation("========================================");

        // Create a logger for MultiCampDataFetcher using the injected ILoggerFactory
        var multiCampLogger = _loggerFactory.CreateLogger<MultiCampDataFetcher>();
        var processorLogger = _loggerFactory.CreateLogger<MultiCampDataProcessor>();
        var exporterLogger = _loggerFactory.CreateLogger<MultiCampDataExcelExporter>();

        // 1. Fetch Data from all camps
        var sw = Stopwatch.StartNew();
        var dataFetcher = new MultiCampDataFetcher(
            _healthCampRepository,
            _intakeFormResponseRepository,
            _healthAssessmentFormService,
            _doctorRecommendationService,
            multiCampLogger);

        var multiCampData = await dataFetcher.FetchAllCampsDataAsync(ct);
        sw.Stop();

        _logger.LogInformation("========================================");
        _logger.LogInformation("📦 PHASE 1 - DATA FETCHING COMPLETED");
        _logger.LogInformation("⏱️ Time: {ElapsedSeconds:F2}s ({ElapsedMs}ms)", sw.Elapsed.TotalSeconds, sw.ElapsedMilliseconds);
        _logger.LogInformation("📊 Camps: {CampCount}, Participants: {ParticipantCount}",
            multiCampData.CampDataList.Count,
            multiCampData.CampDataList.Sum(c => c.EntityResponses.Count));
        _logger.LogInformation("========================================");

        // 2. Process Data
        sw = Stopwatch.StartNew();
        var dataProcessor = new MultiCampDataProcessor(processorLogger);
        var processedData = dataProcessor.Process(multiCampData);
        sw.Stop();

        _logger.LogInformation("========================================");
        _logger.LogInformation("⚙️ PHASE 2 - DATA PROCESSING COMPLETED");
        _logger.LogInformation("⏱️ Time: {ElapsedSeconds:F2}s ({ElapsedMs}ms)", sw.Elapsed.TotalSeconds, sw.ElapsedMilliseconds);
        _logger.LogInformation("📊 Fields: {IntakeFields} intake, {HealthFields} health, {DoctorFields} doctor",
            processedData.IntakeFieldCount,
            processedData.HealthFieldCount,
            processedData.DoctorRecommendationFieldCount);
        _logger.LogInformation("========================================");

        // 3. Export Data
        sw = Stopwatch.StartNew();
        var exporter = new MultiCampDataExcelExporter(exporterLogger);
        var excelData = exporter.Export(processedData);
        sw.Stop();

        _logger.LogInformation("========================================");
        _logger.LogInformation("📄 PHASE 3 - EXCEL GENERATION COMPLETED");
        _logger.LogInformation("⏱️ Time: {ElapsedSeconds:F2}s ({ElapsedMs}ms)", sw.Elapsed.TotalSeconds, sw.ElapsedMilliseconds);
        _logger.LogInformation("📁 File size: {FileSizeKB:F2} KB ({FileSize} bytes)",
            excelData.Length / 1024.0, excelData.Length);
        _logger.LogInformation("========================================");

        totalStopwatch.Stop();

        _logger.LogInformation("========================================");
        _logger.LogInformation("✅ EXPORT COMPLETED SUCCESSFULLY");
        _logger.LogInformation("⏱️ TOTAL TIME: {TotalMinutes:F2} minutes ({TotalSeconds:F2}s)",
            totalStopwatch.Elapsed.TotalMinutes, totalStopwatch.Elapsed.TotalSeconds);
        _logger.LogInformation("========================================");

        return (excelData, exportTimestamp);
    }


}
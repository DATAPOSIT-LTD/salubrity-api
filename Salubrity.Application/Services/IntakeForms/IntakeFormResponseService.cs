#nullable enable

using ClosedXML.Excel;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthAssessments;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.HealthAssessments;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    public IntakeFormResponseService(
        IIntakeFormResponseRepository intakeFormResponseRepository,
        IHealthCampParticipantRepository participantRepository,
        ICampQueueRepository campQueueRepository,
        IHealthCampServiceAssignmentRepository assignmentRepository,
        IServiceRepository serviceRepository,
        IServiceCategoryRepository serviceCategoryRepository,
        IServiceSubcategoryRepository serviceSubcategoryRepository,
        ILogger<IntakeFormResponseService> logger,
        IHealthCampService campService,
        IIntakeFormRepository intakeFormRepository,
        IHealthCampRepository healthCampRepository,
        IHealthAssessmentFormService healthAssessmentFormService
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
        _campService = campService;
        _intakeFormRepository = intakeFormRepository;
        _healthCampRepository = healthCampRepository;
        _healthAssessmentFormService = healthAssessmentFormService;
    }

    public async Task<Guid> SubmitResponseAsync(CreateIntakeFormResponseDto dto, Guid submittedByUserId, CancellationToken ct = default)
    {
        _logger.LogInformation("🚀 Submitting intake form for participant: {ParticipantId}, SubmittedBy: {UserId}", dto.PatientId, submittedByUserId);

        var versionExists = await _intakeFormResponseRepository.IntakeFormVersionExistsAsync(dto.IntakeFormVersionId, ct);
        if (!versionExists)
            throw new NotFoundException("Form version not found.");

        var validFieldIds = await _intakeFormResponseRepository.GetFieldIdsForVersionAsync(dto.IntakeFormVersionId, ct);
        var invalidField = dto.FieldResponses.FirstOrDefault(f => !validFieldIds.Contains(f.FieldId));
        if (invalidField is not null)
            throw new ValidationException([$"Field {invalidField.FieldId} does not belong to form version {dto.IntakeFormVersionId}."]);

        var patientId = await _participantRepository.GetPatientIdByParticipantIdAsync(dto.PatientId, ct);
        if (patientId is null)
            throw new NotFoundException($"Patient not found for participant {dto.PatientId}.");

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

            _logger.LogInformation("📦 Assignment found: Type={Type}, Id={Id}", submittedServiceType, submittedServiceId);

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
            _logger.LogInformation("🧾 Incoming reference Id (from client ServiceId field): {RefId}", incomingRefId);

            // Detect what the incomingRefId actually is and resolve to top-level ServiceId
            if (await _serviceRepository.ExistsByIdAsync(incomingRefId, ct))
            {
                submittedServiceType = PackageItemType.Service;
                submittedServiceId = incomingRefId;
                resolvedServiceId = incomingRefId;
                _logger.LogInformation("🧭 Detected type=Service. Using ServiceId as resolved: {ServiceId}", resolvedServiceId);
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
                    _logger.LogInformation("🧭 Detected type=ServiceCategory. Resolved root ServiceId: {ServiceId}", resolvedServiceId);
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
                        _logger.LogInformation("🧭 Detected type=ServiceSubcategory. Resolved root ServiceId: {ServiceId}", resolvedServiceId);
                    }
                    else
                    {
                        _logger.LogError("❌ Incoming reference Id does not match Service/Category/Subcategory: {RefId}", incomingRefId);
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
                dto.PatientId,
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

            _logger.LogInformation("✅ Marking station check-in as completed: CheckInId={CheckInId}, FinishedAt={FinishedAt}", checkIn.Id, now);
            await _stationCheckInRepository.UpdateAsync(checkIn, ct);
        }

        _logger.LogInformation("✅ Intake form submitted successfully. ResponseId={ResponseId}", responseId);
        return responseId;
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

    public async Task<(byte[] ExcelData, string CampName, string OrganizationName, DateTime ExportTimestamp)> ExportCampDataToExcelAsync(Guid campId, CancellationToken ct = default)
    {
        var exportTimestamp = DateTime.Now.AddHours(3);
        var camp = await _healthCampRepository.GetByIdAsync(campId);
        if (camp == null)
            throw new NotFoundException($"Health camp with ID {campId} not found.");

        string organizationName = camp.Organization?.BusinessName ?? "Unknown_Organization";

        if (organizationName == "Unknown_Organization")
        {
            try
            {
                var campDetails = await _healthCampRepository.GetCampDetailsByIdAsync(campId);
                if (campDetails != null)
                {
                    organizationName = campDetails.ClientName ?? "Unknown_Organization";
                }
            }
            catch
            {
                var participants = await _healthCampRepository.GetParticipantsAsync(campId, null, null, ct);
                if (participants.Any())
                {
                    var firstParticipant = participants.First();
                    organizationName = firstParticipant.HealthCamp?.Organization?.BusinessName ?? "Unknown_Organization";
                }
            }
        }

        var entityResponses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(campId, ct);

        if (!entityResponses.Any())
            throw new NotFoundException("No intake form responses found for this camp.");

        var campParticipants = await _healthCampRepository.GetParticipantsAsync(campId, null, null, ct);
        var campParticipantUserIds = campParticipants.Select(cp => cp.UserId).ToHashSet();

        var filteredEntityResponses = entityResponses
            .Where(r => r.Patient?.User != null && campParticipantUserIds.Contains(r.Patient.UserId))
            .ToList();

        if (!filteredEntityResponses.Any())
            throw new NotFoundException("No intake form responses found for participants in this camp.");

        var patientIds = filteredEntityResponses.Select(r => r.PatientId).Distinct().ToList();

        // Get Intake Form responses
        var allDtoResponses = new List<IntakeFormResponseDetailDto>();
        foreach (var patientId in patientIds)
        {
            var patientDtoResponses = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, campId, ct);
            allDtoResponses.AddRange(patientDtoResponses);
        }

        // Build field structures for both intake forms and health assessments
        var healthAssessmentFields = new List<(string FieldId, string Label, string SectionName, int Order, string FieldType, string DataSource, int SectionPriority)>();
        var healthAssessmentLookup = new Dictionary<Guid, Dictionary<string, string>>();

        // Get sample health assessment structure for building headers (from first patient)
        List<HealthAssessmentResponseDto> sampleHealthAssessmentResponses = new();
        if (patientIds.Any())
        {
            sampleHealthAssessmentResponses = await _healthAssessmentFormService.GetPatientAssessmentResponsesAsync(patientIds.First(), campId, ct);
        }

        // Process Intake Form fields with structured ordering
        var intakeFormFields = new List<(string FieldId, string Label, string SectionName, int Order, string FieldType, string DataSource, int SectionPriority)>();
        var fieldToSectionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dtoResponse in allDtoResponses)
        {
            foreach (var fieldResponse in dtoResponse.FieldResponses)
            {
                var fieldId = $"intake_{fieldResponse.FieldId}";
                var fieldLabel = fieldResponse.Field.Label;
                var sectionName = fieldResponse.Field.SectionName ?? "General";
                var order = fieldResponse.Field.Order;
                var fieldType = fieldResponse.Field.FieldType ?? "text";
                var sectionPriority = GetSectionPriority(sectionName, fieldLabel);

                if (!intakeFormFields.Any(f => f.FieldId == fieldId))
                {
                    intakeFormFields.Add((fieldId, fieldLabel, sectionName, order, fieldType, "IntakeForm", sectionPriority));
                }

                if (!fieldToSectionMap.ContainsKey(fieldLabel))
                {
                    fieldToSectionMap[fieldLabel] = sectionName;
                }
            }
        }

        // Process Health Assessment fields
        foreach (var assessmentResponse in sampleHealthAssessmentResponses)
        {
            foreach (var section in assessmentResponse.Sections)
            {
                foreach (var field in section.Fields)
                {
                    var fieldId = $"health_{assessmentResponse.FormName}_{section.SectionName}_{field.FieldLabel}".Replace(" ", "_");
                    var fieldLabel = field.FieldLabel;
                    var sectionName = $"Health Assessment - {assessmentResponse.FormName} - {section.SectionName}";
                    var order = field.FieldOrder;
                    var sectionPriority = 1000; // Health assessments come after intake forms

                    if (!healthAssessmentFields.Any(f => f.FieldId == fieldId))
                    {
                        healthAssessmentFields.Add((fieldId, fieldLabel, sectionName, order, "text", "HealthAssessment", sectionPriority));
                    }

                    if (!fieldToSectionMap.ContainsKey($"{sectionName} - {fieldLabel}"))
                    {
                        fieldToSectionMap[$"{sectionName} - {fieldLabel}"] = sectionName;
                    }
                }
            }
        }

        // Fallback to entity responses if no DTO responses
        if (!intakeFormFields.Any())
        {
            foreach (var entityResponse in filteredEntityResponses)
            {
                foreach (var fieldResponse in entityResponse.FieldResponses)
                {
                    var fieldId = $"intake_{fieldResponse.FieldId}";
                    var fieldLabel = fieldResponse.Field.Label;
                    var sectionName = fieldResponse.Field.Section?.Name ?? "General";
                    var order = fieldResponse.Field.Order;
                    var fieldType = fieldResponse.Field.FieldType ?? "text";
                    var sectionPriority = GetSectionPriority(sectionName, fieldLabel);

                    if (!intakeFormFields.Any(f => f.FieldId == fieldId))
                    {
                        intakeFormFields.Add((fieldId, fieldLabel, sectionName, order, fieldType, "IntakeForm", sectionPriority));
                    }

                    if (!fieldToSectionMap.ContainsKey(fieldLabel))
                    {
                        fieldToSectionMap[fieldLabel] = sectionName;
                    }
                }
            }
        }

        // Combine and organize all fields with proper ordering
        var allFields = intakeFormFields.Concat(healthAssessmentFields).ToList();

        var fieldsBySection = allFields
            .GroupBy(f => new { f.SectionName, f.SectionPriority })
            .OrderBy(g => g.Key.SectionPriority)
            .ThenBy(g => g.Key.SectionName)
            .ToList();

        var participantResponses = filteredEntityResponses
            .GroupBy(r => r.PatientId)
            .Where(g => g.First().Patient != null)
            .OrderBy(g => g.First().Patient?.User?.FirstName ?? "")
            .ToList();

        var dtoResponseLookup = allDtoResponses
            .GroupBy(r => r.PatientId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Create Health Assessment response lookup
        foreach (var patientId in patientIds)
        {
            var patientHealthResponses = await _healthAssessmentFormService.GetPatientAssessmentResponsesAsync(patientId, campId, ct);

            var fieldValueLookup = new Dictionary<string, string>();
            foreach (var assessmentResponse in patientHealthResponses)
            {
                foreach (var section in assessmentResponse.Sections)
                {
                    foreach (var field in section.Fields)
                    {
                        var fieldId = $"health_{assessmentResponse.FormName}_{section.SectionName}_{field.FieldLabel}".Replace(" ", "_");
                        fieldValueLookup[fieldId] = field.Value ?? field.SelectedOption ?? "";
                    }
                }
            }
            healthAssessmentLookup[patientId] = fieldValueLookup;
        }

        // Create ordered fields list for data population
        var orderedFields = new List<(string FieldId, string Label, string SectionName, string FieldType, string DataSource)>();
        foreach (var sectionGroup in fieldsBySection)
        {
            var fieldsInSection = sectionGroup.OrderBy(f => GetFieldPriority(f.Label)).ThenBy(f => f.Order).ToList();
            foreach (var field in fieldsInSection)
            {
                orderedFields.Add((field.FieldId, field.Label, field.SectionName, field.FieldType, field.DataSource));
            }
        }

        // Create Excel workbook and worksheet
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Camp Data Export");

        // Create structured headers with multi-level approach
        var headerStructure = CreateHeaderStructure(fieldsBySection, orderedFields);
        var totalColumns = SetupStructuredHeaders(worksheet, headerStructure);

        // Start data from row 4 (since we now have 3 header rows)
        int currentRow = 4;
        foreach (var participantGroup in participantResponses)
        {
            var participant = participantGroup.First().Patient;
            if (participant?.User == null) continue;

            // Fill participant basic info (columns 1-8)
            worksheet.Cell(currentRow, 1).Value = participant.User.FullName ?? "";
            worksheet.Cell(currentRow, 2).Value = participant.User.Email ?? "";
            worksheet.Cell(currentRow, 3).Value = participant.User.Phone ?? "";
            worksheet.Cell(currentRow, 4).Value = participant.User.Gender?.Name ?? "";
            worksheet.Cell(currentRow, 5).Value = participant.User.NationalId ?? "";
            worksheet.Cell(currentRow, 6).Value = participant.User.DateOfBirth?.ToString("yyyy-MM-dd") ?? "";

            string ageValue = "";
            if (participant.User.DateOfBirth.HasValue)
            {
                var age = CalculateAge(participant.User.DateOfBirth.Value, DateTime.Now);
                ageValue = age.ToString();
            }
            worksheet.Cell(currentRow, 7).Value = ageValue;

            // Get response lookups for this participant
            Dictionary<string, string> intakeFormResponseLookup;
            if (dtoResponseLookup.TryGetValue(participant.Id, out var dtoList) && dtoList.Any())
            {
                intakeFormResponseLookup = dtoList
                    .SelectMany(r => r.FieldResponses)
                    .GroupBy(fr => $"intake_{fr.FieldId}")
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
            }
            else
            {
                intakeFormResponseLookup = participantGroup
                    .SelectMany(r => r.FieldResponses)
                    .GroupBy(fr => $"intake_{fr.FieldId}")
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
            }

            var healthAssessmentResponseLookup = healthAssessmentLookup.TryGetValue(participant.Id, out var healthResponses)
                ? healthResponses
                : new Dictionary<string, string>();

            // Calculate and set lifestyle risk
            var lifestyleRisk = CalculateLifestyleRisk(intakeFormResponseLookup, healthAssessmentResponseLookup, orderedFields);
            worksheet.Cell(currentRow, 8).Value = lifestyleRisk;
            var riskColor = GetLifestyleRiskColor(lifestyleRisk);
            worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = riskColor;
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            // Fill dynamic field data starting from column 9
            int columnIndex = 9;
            foreach (var field in orderedFields)
            {
                string value = "";

                if (field.DataSource == "IntakeForm")
                {
                    if (intakeFormResponseLookup.TryGetValue(field.FieldId, out var fieldValue))
                    {
                        value = FormatFieldValue(fieldValue, field.FieldType);
                    }
                }
                else if (field.DataSource == "HealthAssessment")
                {
                    if (healthAssessmentResponseLookup.TryGetValue(field.FieldId, out var healthValue))
                    {
                        value = healthValue;
                    }
                }

                worksheet.Cell(currentRow, columnIndex).Value = value;
                columnIndex++;
            }

            currentRow++;
        }

        // Apply styling and formatting
        ApplyWorksheetStyling(worksheet, totalColumns, currentRow - 1, headerStructure);

        // Auto-adjust column widths with limits
        foreach (var column in worksheet.Columns())
        {
            column.AdjustToContents();
            if (column.Width > 40)
                column.Width = 40;
            if (column.Width < 12)
                column.Width = 12;
        }

        // Apply borders and alternating row colors for data
        if (currentRow > 4)
        {
            var dataRange = worksheet.Range(4, 1, currentRow - 1, totalColumns);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            // Alternating row colors
            for (int row = 4; row < currentRow; row++)
            {
                if ((row - 4) % 2 == 0)
                {
                    worksheet.Range(row, 1, row, totalColumns).Style.Fill.BackgroundColor = XLColor.AliceBlue;
                }
            }
        }

        // Add export summary section
        int summaryStartRow = currentRow + 2;
        worksheet.Cell(summaryStartRow, 1).Value = "Export Summary:";
        worksheet.Cell(summaryStartRow, 1).Style.Font.Bold = true;
        worksheet.Cell(summaryStartRow, 1).Style.Font.FontSize = 12;

        worksheet.Cell(summaryStartRow + 1, 1).Value = $"Camp: {camp.Name}";
        worksheet.Cell(summaryStartRow + 2, 1).Value = $"Organization: {organizationName}";
        worksheet.Cell(summaryStartRow + 3, 1).Value = $"Total Participants: {participantResponses.Count}";
        worksheet.Cell(summaryStartRow + 4, 1).Value = $"Total Intake Form Fields: {intakeFormFields.Count}";
        worksheet.Cell(summaryStartRow + 5, 1).Value = $"Total Health Assessment Fields: {healthAssessmentFields.Count}";
        worksheet.Cell(summaryStartRow + 6, 1).Value = $"Total Sections: {fieldsBySection.Count}";
        worksheet.Cell(summaryStartRow + 7, 1).Value = $"Export Date: {DateTime.Now.AddHours(3):yyyy-MM-dd HH:mm:ss}";

        var summaryRange = worksheet.Range(summaryStartRow, 1, summaryStartRow + 7, 2);
        summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        summaryRange.Style.Fill.BackgroundColor = XLColor.LightYellow;

        // Add section color legend
        int legendStartRow = summaryStartRow + 9;
        worksheet.Cell(legendStartRow, 1).Value = "Section Color Legend:";
        worksheet.Cell(legendStartRow, 1).Style.Font.Bold = true;

        int legendRow = legendStartRow + 1;
        var uniqueSections = fieldsBySection.Select(g => g.Key.SectionName).Distinct().ToList();
        foreach (var sectionName in uniqueSections)
        {
            worksheet.Cell(legendRow, 1).Value = sectionName;
            worksheet.Cell(legendRow, 1).Style.Fill.BackgroundColor = GetSectionColor(sectionName);
            worksheet.Cell(legendRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            legendRow++;
        }

        // Freeze panes: 3 header rows and 8 basic info columns
        worksheet.SheetView.Freeze(3, 4);

        // Save and return
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return (stream.ToArray(), camp.Name, organizationName, exportTimestamp);
    }

    private static int GetSectionPriority(string sectionName, string fieldLabel)
    {
        // Define the structured ordering based on our proposed arrangement
        return sectionName.ToLowerInvariant() switch
        {
            var s when s.Contains("diagnosis") && s.Contains("examination") => 100, // Physical Measurements & Vital Signs
            var s when s.Contains("physical examination") => 200, // Physical Examination
            var s when s.Contains("eye") || s.Contains("vision") => 300, // Eye Examination
            var s when s.Contains("nutrition") => 400, // Nutrition Assessment
            var s when s.Contains("mental health") => 500, // Mental Health
            var s when s.Contains("back") || s.Contains("muscle") || s.Contains("nerve") || s.Contains("bone") => 510, // Back, Muscles, Nerves & Bones
            var s when s.Contains("well woman") => 520, // Well Woman
            var s when s.Contains("findings") => 600, // General Findings
            var s when s.Contains("diagnosis") && !s.Contains("examination") => 610, // General Diagnosis
            var s when s.Contains("notes") => 700, // Notes
            var s when s.Contains("recommendations") => 800, // Recommendations
            _ => 900 // Other/General sections
        };
    }

    private static int GetFieldPriority(string fieldLabel)
    {
        // Define field-level priority within sections for better organization
        var label = fieldLabel.ToLowerInvariant();

        // Vital Signs ordering
        if (label.Contains("height")) return 1;
        if (label.Contains("weight")) return 2;
        if (label.Contains("bmi")) return 3;
        if (label.Contains("temperature")) return 4;
        if (label.Contains("heart rate")) return 5;
        if (label.Contains("blood pressure") && !label.Contains("reading")) return 6;
        if (label.Contains("bp reading 2")) return 7;
        if (label.Contains("bp reading 3")) return 8;
        if (label.Contains("oxygen")) return 9;
        if (label.Contains("blood sugar") && !label.Contains("reading")) return 10;
        if (label.Contains("rbs reading 2")) return 11;
        if (label.Contains("rbs reading 3")) return 12;

        // Physical Examination ordering
        if (label.Contains("rs")) return 1;
        if (label.Contains("cvs")) return 2;
        if (label.Contains("abdomen")) return 3;
        if (label.Contains("cns")) return 4;
        if (label.Contains("skin")) return 5;

        // Eye examination ordering (Right eye first, then Left eye)
        if (label.Contains("right eye")) return 1;
        if (label.Contains("left eye")) return 2;
        if (label.Contains("vision")) return 1;
        if (label.Contains("sphere")) return 2;
        if (label.Contains("cylinder")) return 3;
        if (label.Contains("axis")) return 4;
        if (label.Contains("va")) return 5;
        if (label.Contains("add")) return 6;

        // Diagnosis sections ordering
        if (label.Contains("findings")) return 1;
        if (label.Contains("conclusion")) return 2;
        if (label.Contains("recommendations")) return 3;
        if (label.Contains("additional notes")) return 4;

        // Nutrition ordering
        if (label.Contains("body mass index")) return 1;
        if (label.Contains("body fat")) return 2;
        if (label.Contains("body water")) return 3;
        if (label.Contains("muscle mass")) return 4;
        if (label.Contains("basal metabolic")) return 5;
        if (label.Contains("bone density")) return 6;
        if (label.Contains("visceral fat")) return 7;
        if (label.Contains("metabolic age")) return 8;
        if (label.Contains("problem")) return 9;
        if (label.Contains("plan")) return 10;

        return 50; // Default priority for other fields
    }

    private static XLColor GetSectionColor(string sectionName)
    {
        return sectionName.ToLowerInvariant() switch
        {
            var s when s.Contains("personal") || s.Contains("demographic") => XLColor.LightBlue,
            var s when s.Contains("medical") || s.Contains("health") => XLColor.LightGreen,
            var s when s.Contains("assessment") || s.Contains("evaluation") => XLColor.LightYellow,
            var s when s.Contains("vitals") || s.Contains("vital") => XLColor.LightPink,
            var s when s.Contains("lab") || s.Contains("laboratory") => XLColor.LightCyan,
            var s when s.Contains("history") || s.Contains("background") => XLColor.Lavender,
            var s when s.Contains("physical") || s.Contains("examination") => XLColor.LightSalmon,
            var s when s.Contains("mental") || s.Contains("psychological") => XLColor.LightSteelBlue,
            var s when s.Contains("nutrition") || s.Contains("dietary") => XLColor.LightGoldenrodYellow,
            var s when s.Contains("recommendation") || s.Contains("advice") => XLColor.LightSeaGreen,
            "general" => XLColor.WhiteSmoke,
            _ => XLColor.LightGray
        };
    }

    private static int CalculateAge(DateTime birthDate, DateTime currentDate)
    {
        int age = currentDate.Year - birthDate.Year;

        // Check if birthday has occurred this year
        if (currentDate.Month < birthDate.Month ||
            (currentDate.Month == birthDate.Month && currentDate.Day < birthDate.Day))
        {
            age--;
        }

        return Math.Max(0, age); // Ensure age is never negative
    }


    private static string CalculateLifestyleRisk(
        Dictionary<string, string> intakeFormResponses,
        Dictionary<string, string> healthAssessmentResponses,
        List<(string FieldId, string Label, string SectionName, string FieldType, string DataSource)> orderedFields)
    {
        // Extract all available values from both data sources
        var allValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Add intake form values
        foreach (var kvp in intakeFormResponses)
        {
            var field = orderedFields.FirstOrDefault(f => f.FieldId == kvp.Key);
            if (field.Label != null)
            {
                allValues[field.Label] = kvp.Value;
            }
        }

        // Add health assessment values
        foreach (var kvp in healthAssessmentResponses)
        {
            var field = orderedFields.FirstOrDefault(f => f.FieldId == kvp.Key);
            if (field.Label != null)
            {
                allValues[field.Label] = kvp.Value;
            }
        }

        // Calculate individual risk scores
        var bmiRisk = CalculateBMIRisk(allValues);
        var bloodPressureRisk = CalculateBloodPressureRisk(allValues);
        var bloodGlucoseRisk = CalculateBloodGlucoseRisk(allValues);
        var cholesterolRisk = CalculateCholesterolRisk(allValues);

        // Convert risk levels to numeric scores for calculation
        var riskScores = new List<int>
        {
            ConvertRiskToScore(bmiRisk),
            ConvertRiskToScore(bloodPressureRisk),
            ConvertRiskToScore(bloodGlucoseRisk),
            ConvertRiskToScore(cholesterolRisk)
        };

        // Filter out invalid scores (0 means no data available)
        var validScores = riskScores.Where(score => score > 0).ToList();

        if (!validScores.Any())
        {
            return "No Data";
        }

        // Calculate average risk score
        var averageScore = validScores.Average();

        // Convert back to risk category
        var calculatedRisk = ConvertScoreToRisk(averageScore);

        // Check for high-risk factors that prevent Very Low or Low risk classification
        var hasChronicDisease = CheckForChronicDisease(allValues);
        var hasAlcoholUse = CheckForAlcoholUse(allValues);
        var hasSmokingHabit = CheckForSmokingHabit(allValues);

        // If any of the three high-risk factors are present, minimum risk is Medium
        if (hasChronicDisease || hasAlcoholUse || hasSmokingHabit)
        {
            if (calculatedRisk == "Very Low" || calculatedRisk == "Low")
            {
                return "Medium";
            }
        }

        return calculatedRisk;
    }

    private static string CalculateBMIRisk(Dictionary<string, string> values)
    {
        // First try to find direct BMI value
        var bmiValue = FindValueByKeywords(values, new[] { "bmi", "body mass index" });

        if (!string.IsNullOrEmpty(bmiValue) && decimal.TryParse(bmiValue, out var bmi))
        {
            return GetBMIRiskCategory(bmi);
        }

        // If no direct BMI, try to calculate from height and weight
        var heightValue = FindValueByKeywords(values, new[] { "height" });
        var weightValue = FindValueByKeywords(values, new[] { "weight" });

        if (!string.IsNullOrEmpty(heightValue) && !string.IsNullOrEmpty(weightValue) &&
            decimal.TryParse(heightValue, out var height) && decimal.TryParse(weightValue, out var weight))
        {
            if (height > 0)
            {
                // Convert height from cm to meters if needed
                var heightInMeters = height > 10 ? height / 100 : height;
                var calculatedBMI = weight / (heightInMeters * heightInMeters);
                return GetBMIRiskCategory(calculatedBMI);
            }
        }

        return "No Data";
    }

    private static string CalculateBloodPressureRisk(Dictionary<string, string> values)
    {
        // Look for blood pressure readings
        var bpValue = FindValueByKeywords(values, new[] { "blood pressure", "bp reading", "systolic", "diastolic" });

        if (string.IsNullOrEmpty(bpValue))
        {
            return "No Data";
        }

        // Parse blood pressure format (e.g., "120/80", "120", "80")
        var systolic = 0m;
        var diastolic = 0m;

        if (bpValue.Contains("/"))
        {
            var parts = bpValue.Split('/');
            if (parts.Length == 2 &&
                decimal.TryParse(parts[0].Trim(), out systolic) &&
                decimal.TryParse(parts[1].Trim(), out diastolic))
            {
                return GetBloodPressureRiskCategory(systolic, diastolic);
            }
        }
        else if (decimal.TryParse(bpValue, out var singleValue))
        {
            // If only one value, assume it's systolic
            systolic = singleValue;
            return GetBloodPressureRiskCategory(systolic, 0);
        }

        return "No Data";
    }

    private static string CalculateBloodGlucoseRisk(Dictionary<string, string> values)
    {
        var glucoseValue = FindValueByKeywords(values, new[] { "blood sugar", "glucose", "rbs", "random blood sugar" });

        if (!string.IsNullOrEmpty(glucoseValue) && decimal.TryParse(glucoseValue, out var glucose))
        {
            return GetBloodGlucoseRiskCategory(glucose);
        }

        return "No Data";
    }

    private static string CalculateCholesterolRisk(Dictionary<string, string> values)
    {
        var cholesterolValue = FindValueByKeywords(values, new[] { "cholesterol", "total cholesterol" });

        if (!string.IsNullOrEmpty(cholesterolValue) && decimal.TryParse(cholesterolValue, out var cholesterol))
        {
            return GetCholesterolRiskCategory(cholesterol);
        }

        return "No Data";
    }

    private static bool CheckForChronicDisease(Dictionary<string, string> allValues)
    {
        // Check for chronic disease response
        var chronicDiseaseKey = "Do you suffer from any chronic disease? Diabetes, hypertension, thyroid, cancer, heart disease, stroke. If others, which one?";

        if (allValues.TryGetValue(chronicDiseaseKey, out var chronicResponse))
        {
            // If response is not empty, null, "No", or "None", consider it as having chronic disease
            return !string.IsNullOrWhiteSpace(chronicResponse) &&
                   !chronicResponse.Equals("No", StringComparison.OrdinalIgnoreCase) &&
                   !chronicResponse.Equals("None", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool CheckForAlcoholUse(Dictionary<string, string> allValues)
    {
        // Check for alcohol use response
        var alcoholKey = "Do you take alcohol?";

        if (allValues.TryGetValue(alcoholKey, out var alcoholResponse))
        {
            // If response is "Yes" or any positive indication, consider it as alcohol use
            return !string.IsNullOrWhiteSpace(alcoholResponse) &&
                   !alcoholResponse.Equals("No", StringComparison.OrdinalIgnoreCase) &&
                   !alcoholResponse.Equals("None", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool CheckForSmokingHabit(Dictionary<string, string> allValues)
    {
        // Check for smoking response
        var smokingKey = "Do you smoke? If yes, Count sticks per day?";

        if (allValues.TryGetValue(smokingKey, out var smokingResponse))
        {
            // If response is not empty, null, "No", or "0", consider it as smoking
            return !string.IsNullOrWhiteSpace(smokingResponse) &&
                   !smokingResponse.Equals("No", StringComparison.OrdinalIgnoreCase) &&
                   !smokingResponse.Equals("0", StringComparison.OrdinalIgnoreCase) &&
                   !smokingResponse.Equals("None", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static string FindValueByKeywords(Dictionary<string, string> values, string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            var matchingKey = values.Keys.FirstOrDefault(key =>
                key.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (matchingKey != null && !string.IsNullOrEmpty(values[matchingKey]))
            {
                return values[matchingKey];
            }
        }
        return "";
    }

    private static string GetBMIRiskCategory(decimal bmi)
    {
        return bmi switch
        {
            >= 18 and <= 22.9m => "Very Low",
            >= 23 and <= 24.9m => "Low",
            >= 25 and <= 27.9m => "Medium",
            >= 28 and <= 30m => "High",
            > 30 => "Very High",
            _ => "No Data"
        };
    }

    private static string GetBloodPressureRiskCategory(decimal systolic, decimal diastolic)
    {
        // Use systolic as primary indicator if diastolic is 0 or not available
        if (diastolic == 0)
        {
            return systolic switch
            {
                < 120 => "Very Low",
                >= 120 and <= 139 => "Low",
                >= 140 and <= 150 => "Medium",
                >= 151 and <= 159 => "High",
                >= 160 => "Very High",
                _ => "No Data"
            };
        }

        // Use both systolic and diastolic when available
        if (systolic < 120 && diastolic < 80) return "Very Low";
        if (systolic <= 139 && diastolic <= 89) return "Low";
        if (systolic <= 150 && diastolic <= 95) return "Medium";
        if (systolic <= 159 && diastolic <= 99) return "High";
        if (systolic >= 160 || diastolic >= 100) return "Very High";

        return "No Data";
    }

    private static string GetBloodGlucoseRiskCategory(decimal glucose)
    {
        return glucose switch
        {
            < 7 => "Very Low",
            >= 7 and <= 10 => "Medium",
            >= 10 and <= 10.9m => "High",
            > 11 => "Very High",
            _ => "No Data"
        };
    }

    private static string GetCholesterolRiskCategory(decimal cholesterol)
    {
        return cholesterol switch
        {
            >= 2.3m and <= 4.9m => "Very Low",
            >= 5 and <= 5.17m => "Low",
            >= 5.18m and <= 6.19m => "Medium",
            > 6.2m and <= 7m => "High",
            > 7 => "Very High",
            _ => "No Data"
        };
    }

    private static int ConvertRiskToScore(string riskLevel)
    {
        return riskLevel switch
        {
            "Very Low" => 1,
            "Low" => 2,
            "Medium" => 3,
            "High" => 4,
            "Very High" => 5,
            _ => 0 // No Data
        };
    }

    private static string ConvertScoreToRisk(double averageScore)
    {
        return averageScore switch
        {
            <= 1.5 => "Very Low",
            <= 2.5 => "Low",
            <= 3.5 => "Medium",
            <= 4.5 => "High",
            _ => "Very High"
        };
    }

    private static XLColor GetLifestyleRiskColor(string riskLevel)
    {
        return riskLevel switch
        {
            "Very Low" => XLColor.LightGreen,
            "Low" => XLColor.LightYellow,
            "Medium" => XLColor.Orange,
            "High" => XLColor.LightCoral,
            "Very High" => XLColor.Red,
            _ => XLColor.LightGray // No Data
        };
    }

    private HeaderStructure CreateHeaderStructure(
    IEnumerable<IGrouping<dynamic, (string FieldId, string Label, string SectionName, int Order, string FieldType, string DataSource, int SectionPriority)>> fieldsBySection,
    List<(string FieldId, string Label, string SectionName, string FieldType, string DataSource)> orderedFields)
    {
        var structure = new HeaderStructure();

        // Add fixed participant columns
        structure.AddSection("Participant Information", new List<HeaderColumn>
        {
            new("Name", "Name"),
            new("Email", "Email"),
            new("Phone", "Phone"),
            new("Gender", "Gender"),
            new("ID Number", "ID"),
            new("Date of Birth", "DOB"),
            new("Age", "Age"),
            new("Lifestyle Risk", "Risk")
        });

        // Group fields by main section and subsection
        foreach (var sectionGroup in fieldsBySection)
        {
            var fieldsInSection = sectionGroup.OrderBy(f => GetFieldPriority(f.Label)).ThenBy(f => f.Order).ToList();

            var mainSectionName = GetMainSectionName(sectionGroup.Key.SectionName);
            var subSectionName = GetSubSectionName(sectionGroup.Key.SectionName);

            var columns = fieldsInSection.Select(field => new HeaderColumn(
                field.Label,
                TruncateFieldName(field.Label), // Shortened version for display
                subSectionName
            )).ToList();

            structure.AddSection(mainSectionName, columns);
        }

        return structure;
    }

    private int SetupStructuredHeaders(IXLWorksheet worksheet, HeaderStructure headerStructure)
    {
        int currentCol = 1;

        foreach (var section in headerStructure.Sections)
        {
            int sectionStartCol = currentCol;

            foreach (var subSection in section.SubSections)
            {
                int subSectionStartCol = currentCol;

                foreach (var column in subSection.Columns)
                {
                    // Row 3: Field names (shortened)
                    worksheet.Cell(3, currentCol).Value = column.ShortName;
                    worksheet.Cell(3, currentCol).Style.Font.Bold = true;
                    worksheet.Cell(3, currentCol).Style.Alignment.WrapText = true;
                    worksheet.Cell(3, currentCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    currentCol++;
                }

                // Row 2: Sub-section headers (if applicable)
                if (subSection.Columns.Count > 1 && !string.IsNullOrEmpty(subSection.Name))
                {
                    worksheet.Range(2, subSectionStartCol, 2, currentCol - 1).Merge();
                    worksheet.Cell(2, subSectionStartCol).Value = subSection.Name;
                    worksheet.Cell(2, subSectionStartCol).Style.Font.Bold = true;
                    worksheet.Cell(2, subSectionStartCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(2, subSectionStartCol).Style.Fill.BackgroundColor = GetSectionColor(subSection.Name);
                }
                else if (subSection.Columns.Count == 1)
                {
                    worksheet.Cell(2, subSectionStartCol).Value = subSection.Name ?? "";
                    worksheet.Cell(2, subSectionStartCol).Style.Font.Bold = true;
                    worksheet.Cell(2, subSectionStartCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(2, subSectionStartCol).Style.Fill.BackgroundColor = GetSectionColor(subSection.Name ?? "");
                }
            }

            // Row 1: Main section headers
            if (currentCol - sectionStartCol > 1)
            {
                worksheet.Range(1, sectionStartCol, 1, currentCol - 1).Merge();
                worksheet.Cell(1, sectionStartCol).Value = section.Name;
                worksheet.Cell(1, sectionStartCol).Style.Font.Bold = true;
                worksheet.Cell(1, sectionStartCol).Style.Font.FontSize = 12;
                worksheet.Cell(1, sectionStartCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, sectionStartCol).Style.Fill.BackgroundColor = GetMainSectionColor(section.Name);
            }
            else
            {
                worksheet.Cell(1, sectionStartCol).Value = section.Name;
                worksheet.Cell(1, sectionStartCol).Style.Font.Bold = true;
                worksheet.Cell(1, sectionStartCol).Style.Font.FontSize = 12;
                worksheet.Cell(1, sectionStartCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, sectionStartCol).Style.Fill.BackgroundColor = GetMainSectionColor(section.Name);
            }
        }

        return currentCol - 1;
    }

    private void ApplyWorksheetStyling(IXLWorksheet worksheet, int totalColumns, int totalRows, HeaderStructure headerStructure)
    {
        // Style header rows
        var headerRange = worksheet.Range(1, 1, 3, totalColumns);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Font.Bold = true;

        // Style data rows
        if (totalRows > 3)
        {
            var dataRange = worksheet.Range(4, 1, totalRows, totalColumns);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            // Alternate row colors
            for (int row = 4; row <= totalRows; row++)
            {
                if ((row - 4) % 2 == 0)
                {
                    worksheet.Range(row, 1, row, totalColumns).Style.Fill.BackgroundColor = XLColor.AliceBlue;
                }
            }
        }

        // Auto-fit columns with limits
        foreach (var column in worksheet.Columns())
        {
            column.AdjustToContents();
            if (column.Width > 25) column.Width = 25;
            if (column.Width < 8) column.Width = 8;
        }

        // Set header row heights
        worksheet.Row(1).Height = 25;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 30;
    }

    private string GetMainSectionName(string fullSectionName)
    {
        if (fullSectionName.Contains("Health Assessment"))
        {
            return "Health Assessments";
        }

        return fullSectionName.Split('-')[0].Trim();
    }

    private string GetSubSectionName(string fullSectionName)
    {
        if (fullSectionName.Contains("Health Assessment"))
        {
            var parts = fullSectionName.Split(" - ");
            if (parts.Length >= 3)
            {
                return $"{parts[1]} - {parts[2]}"; // FormName - SectionName
            }
            return parts.Length > 1 ? parts[1] : fullSectionName;
        }

        return fullSectionName;
    }


    private string TruncateFieldName(string fieldName)
    {
        // Create shorter, more readable field names without truncating
        var commonReplacements = new Dictionary<string, string>
    {
        { "Do you have", "Have" },
        { "Have you ever", "Ever" },
        { "Are you currently", "Currently" },
        { "Please specify", "Specify" },
        { "How many", "Count" },
        { "What is your", "Your" },
        { "Date of Birth", "DOB" },
        { "Blood Pressure", "BP" },
        { "Heart Rate", "HR" },
        { "Body Mass Index", "BMI" },
        { "Temperature", "Temp" },
        { "medication", "meds" },
        { "family history", "family hist" },
        { "medical history", "med hist" }
    };

        string shortened = fieldName;
        foreach (var replacement in commonReplacements)
        {
            shortened = shortened.Replace(replacement.Key, replacement.Value, StringComparison.OrdinalIgnoreCase);
        }

        // Clean up spacing
        shortened = shortened
            .Replace("  ", " ")
            .Trim();

        return shortened;
    }

    private XLColor GetMainSectionColor(string sectionName)
    {
        return sectionName.ToLowerInvariant() switch
        {
            "participant information" => XLColor.LightGray,
            "health assessments" => XLColor.LightBlue,
            "personal information" => XLColor.LightGreen,
            "medical history" => XLColor.LightCoral,
            "lifestyle" => XLColor.LightYellow,
            "emergency contact" => XLColor.LightPink,
            _ => XLColor.LightCyan
        };
    }

    private string FormatFieldValue(string value, string fieldType)
    {
        return fieldType.ToLowerInvariant() switch
        {
            "checkbox" => value == "true" ? "Yes" : value == "false" ? "No" : value,
            "radio" => value,
            "select" => value,
            "multiselect" => value,
            "date" => DateTime.TryParse(value, out var date) ? date.ToString("yyyy-MM-dd") : value,
            "datetime" => DateTime.TryParse(value, out var datetime) ? datetime.ToString("yyyy-MM-dd HH:mm") : value,
            "number" => decimal.TryParse(value, out var number) ? number.ToString("0.##") : value,
            "email" => value,
            "phone" => value,
            "url" => value,
            "textarea" => value,
            "text" => value,
            _ => value
        };
    }

    // Supporting classes for header structure
    public class HeaderStructure
    {
        public List<HeaderSection> Sections { get; set; } = new();

        public void AddSection(string name, List<HeaderColumn> columns)
        {
            var section = Sections.FirstOrDefault(s => s.Name == name);
            if (section == null)
            {
                section = new HeaderSection { Name = name };
                Sections.Add(section);
            }

            // Group columns by subsection
            var groupedColumns = columns.GroupBy(c => c.SubSection ?? "General").ToList();

            foreach (var group in groupedColumns)
            {
                var subSection = new HeaderSubSection
                {
                    Name = group.Key == "General" ? "" : group.Key,
                    Columns = group.ToList()
                };
                section.SubSections.Add(subSection);
            }
        }
    }

    public class HeaderSection
    {
        public string Name { get; set; } = "";
        public List<HeaderSubSection> SubSections { get; set; } = new();
    }

    public class HeaderSubSection
    {
        public string Name { get; set; } = "";
        public List<HeaderColumn> Columns { get; set; } = new();
    }

    public class HeaderColumn
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string? SubSection { get; set; }

        public HeaderColumn(string fullName, string shortName, string? subSection = null)
        {
            FullName = fullName;
            ShortName = shortName;
            SubSection = subSection;
        }
    }
}
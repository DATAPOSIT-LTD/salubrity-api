#nullable enable

using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Shared.Exceptions;
using ClosedXML.Excel;
using System.IO;
using System;
using System.Collections.Generic;
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
        IHealthCampRepository healthCampRepository
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

    public async Task<(byte[] ExcelData, string CampName, string OrganizationName)> ExportCampDataToExcelAsync(Guid campId, CancellationToken ct = default)
    {
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

        var allDtoResponses = new List<IntakeFormResponseDetailDto>();
        foreach (var patientId in patientIds)
        {
            var patientDtoResponses = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, campId, ct);
            allDtoResponses.AddRange(patientDtoResponses);
        }

        var allFieldsInfo = new List<(string FieldId, string Label, string SectionName, int Order, string FieldType)>();
        var fieldToSectionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dtoResponse in allDtoResponses)
        {
            foreach (var fieldResponse in dtoResponse.FieldResponses)
            {
                var fieldId = fieldResponse.FieldId.ToString();
                var fieldLabel = fieldResponse.Field.Label;
                var sectionName = fieldResponse.Field.SectionName ?? "General";
                var order = fieldResponse.Field.Order;
                var fieldType = fieldResponse.Field.FieldType ?? "text";

                if (!allFieldsInfo.Any(f => f.FieldId == fieldId))
                {
                    allFieldsInfo.Add((fieldId, fieldLabel, sectionName, order, fieldType));
                }

                if (!fieldToSectionMap.ContainsKey(fieldLabel))
                {
                    fieldToSectionMap[fieldLabel] = sectionName;
                }
            }
        }

        if (!allFieldsInfo.Any())
        {
            foreach (var entityResponse in filteredEntityResponses)
            {
                foreach (var fieldResponse in entityResponse.FieldResponses)
                {
                    var fieldId = fieldResponse.FieldId.ToString();
                    var fieldLabel = fieldResponse.Field.Label;
                    var sectionName = fieldResponse.Field.Section?.Name ?? "General";
                    var order = fieldResponse.Field.Order;
                    var fieldType = fieldResponse.Field.FieldType ?? "text";

                    if (!allFieldsInfo.Any(f => f.FieldId == fieldId))
                    {
                        allFieldsInfo.Add((fieldId, fieldLabel, sectionName, order, fieldType));
                    }

                    if (!fieldToSectionMap.ContainsKey(fieldLabel))
                    {
                        fieldToSectionMap[fieldLabel] = sectionName;
                    }
                }
            }
        }

        var fieldsBySection = allFieldsInfo
            .GroupBy(f => f.SectionName)
            .OrderBy(g => g.Key)
            .ToList();

        var participantResponses = filteredEntityResponses
            .GroupBy(r => r.PatientId)
            .Where(g => g.First().Patient != null) 
            .OrderBy(g => g.First().Patient?.User?.FirstName ?? "")
            .ToList();

        var dtoResponseLookup = allDtoResponses
            .GroupBy(r => r.PatientId)
            .ToDictionary(g => g.Key, g => g.ToList());

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Camp Data Export");

        var headers = new List<string> { "Participant Name", "Email", "Phone" };
        var orderedFields = new List<(string FieldId, string Label, string SectionName, string FieldType)>();

        foreach (var sectionGroup in fieldsBySection)
        {
            foreach (var field in sectionGroup.OrderBy(f => f.Order))
            {
                headers.Add($"{field.SectionName} - {field.Label}");
                orderedFields.Add((field.FieldId, field.Label, field.SectionName, field.FieldType));
            }
        }

        for (int i = 0; i < headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;

            if (i >= 3) 
            {
                var headerParts = headers[i].Split(" - ", 2);
                var sectionName = headerParts.Length > 1 ? headerParts[0] : "General";
                var color = GetSectionColor(sectionName);
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = color;
            }
            else
            {
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }
        }

        int currentRow = 2;
        foreach (var participantGroup in participantResponses)
        {
            var participant = participantGroup.First().Patient;
            if (participant?.User == null) continue;

            worksheet.Cell(currentRow, 1).Value = participant.User.FullName ?? "";
            worksheet.Cell(currentRow, 2).Value = participant.User.Email ?? "";
            worksheet.Cell(currentRow, 3).Value = participant.User.Phone ?? "";

            Dictionary<string, string> fieldResponseLookup;

            if (dtoResponseLookup.TryGetValue(participant.Id, out var dtoList) && dtoList.Any())
            {
                fieldResponseLookup = dtoList
                    .SelectMany(r => r.FieldResponses)
                    .GroupBy(fr => fr.FieldId.ToString())
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
            }
            else
            {
                fieldResponseLookup = participantGroup
                    .SelectMany(r => r.FieldResponses)
                    .GroupBy(fr => fr.FieldId.ToString())
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
            }

            int columnIndex = 4; 
            foreach (var field in orderedFields)
            {
                string value = "";
                if (fieldResponseLookup.TryGetValue(field.FieldId, out var fieldValue))
                {
                    value = fieldValue;

                    value = field.FieldType.ToLowerInvariant() switch
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

                worksheet.Cell(currentRow, columnIndex).Value = value;
                columnIndex++;
            }

            currentRow++;
        }

        foreach (var column in worksheet.Columns())
        {
            column.AdjustToContents();
            if (column.Width > 50)
                column.Width = 50;
            if (column.Width < 10)
                column.Width = 10;
        }

        var headerRange = worksheet.Range(1, 1, 1, headers.Count);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Font.Bold = true;

        if (currentRow > 2)
        {
            var dataRange = worksheet.Range(2, 1, currentRow - 1, headers.Count);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            for (int row = 2; row < currentRow; row++)
            {
                if (row % 2 == 0)
                {
                    worksheet.Range(row, 1, row, headers.Count).Style.Fill.BackgroundColor = XLColor.AliceBlue;
                }
            }
        }

        int summaryStartRow = currentRow + 2;
        worksheet.Cell(summaryStartRow, 1).Value = "Export Summary:";
        worksheet.Cell(summaryStartRow, 1).Style.Font.Bold = true;
        worksheet.Cell(summaryStartRow, 1).Style.Font.FontSize = 12;

        worksheet.Cell(summaryStartRow + 1, 1).Value = $"Camp: {camp.Name}";
        worksheet.Cell(summaryStartRow + 2, 1).Value = $"Organization: {organizationName}";
        worksheet.Cell(summaryStartRow + 3, 1).Value = $"Total Participants: {participantResponses.Count}";
        worksheet.Cell(summaryStartRow + 4, 1).Value = $"Total Fields: {orderedFields.Count}";
        worksheet.Cell(summaryStartRow + 5, 1).Value = $"Total Sections: {fieldsBySection.Count}";
        worksheet.Cell(summaryStartRow + 6, 1).Value = $"Export Date: {DateTime.Now.AddHours(3):yyyy-MM-dd HH:mm:ss}";

        var summaryRange = worksheet.Range(summaryStartRow, 1, summaryStartRow + 6, 2);
        summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        summaryRange.Style.Fill.BackgroundColor = XLColor.LightYellow;

        int legendStartRow = summaryStartRow + 8;
        worksheet.Cell(legendStartRow, 1).Value = "Section Color Legend:";
        worksheet.Cell(legendStartRow, 1).Style.Font.Bold = true;

        int legendRow = legendStartRow + 1;
        var uniqueSections = fieldsBySection.Select(g => g.Key).Distinct().ToList();
        foreach (var sectionName in uniqueSections)
        {
            worksheet.Cell(legendRow, 1).Value = sectionName;
            worksheet.Cell(legendRow, 1).Style.Fill.BackgroundColor = GetSectionColor(sectionName);
            worksheet.Cell(legendRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            legendRow++;
        }

        worksheet.SheetView.Freeze(1, 3);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return (stream.ToArray(), camp.Name, organizationName);
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
}
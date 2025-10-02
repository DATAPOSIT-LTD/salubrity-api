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
        // Verify camp exists
        var camp = await _healthCampRepository.GetByIdAsync(campId);
        if (camp == null)
            throw new NotFoundException($"Health camp with ID {campId} not found.");

        // Get all responses for this camp with participant and field details
        var responses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(campId, ct);

        if (!responses.Any())
            throw new NotFoundException("No intake form responses found for this camp.");

        // Group responses by participant to consolidate multiple forms per participant
        var participantResponses = responses
            .GroupBy(r => r.PatientId)
            .OrderBy(g => g.First().Patient?.User?.FirstName ?? "")
            .ThenBy(g => g.First().Patient?.User?.LastName ?? "")
            .ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Camp Data Export");

        // Define column headers in the exact order specified
        var headers = new[]
        {
        "Name", "Sex", "YOB", "Age (yrs)", "Workplace", "Lifestyle Risk",
        "Sys BP (mmHg)", "Dia BP (mmHg)", "BP Risk", "HR", "Temp", "RBS (mmol/L)",
        "Diabetes Mellitus Risk", "Height (m)", "Weight (kg)", "BMI (kg/m2)", "BMI Risk",
        "SPO2", "RS", "CVS", "MSS", "CNS", "Mental health screen", "Skin", "Breast",
        "Pap", "ECG", "Visual acuity", "Colour vision", "Audiometry", "Dental",
        "Body Fat%", "Body Water%", "Muscle Mass (%)", "Bone Density (%)", "BMR Kcl/day",
        "Metabolic Age", "Nutrition Review", "FHG", "HbA1c", "HbA1c Flags", "TSH",
        "TSH FLAG", "Urinalysis", "Occult blood", "PSA", "PSA Risk", "Cholesterol (mg/dL)",
        "Lipid Risk", "HDL (mg/dL)", "HDL Risk", "TG (mg/dL)", "TG Risk", "LDL (mg/dL)",
        "LDL Risk", "Creatinine (mg/dL)", "Creat flag", "GGT (u/L)", "GGT flag",
        "Pertinent History", "Clinical findings", "Conclusion", "Recommendation",
        "Specific Instructions", "CDMP"
    };

        // Set headers
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int currentRow = 2;

        foreach (var participantGroup in participantResponses)
        {
            var participant = participantGroup.First().Patient;
            if (participant?.User == null) continue;

            // Collect all field responses for this participant across all their forms
            var allFieldResponses = participantGroup
                .SelectMany(r => r.FieldResponses)
                .GroupBy(fr => fr.Field.Label, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Value ?? "-", StringComparer.OrdinalIgnoreCase);

            // Calculate age from date of birth
            var age = participant.User.DateOfBirth.HasValue
                ? DateTime.Now.Year - participant.User.DateOfBirth.Value.Year
                : (int?)null;

            // Populate row data
            var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Name"] = $"{participant.User.FirstName} {participant.User.MiddleName} {participant.User.LastName}".Replace("  ", " ").Trim(),
                ["Sex"] = GetFieldValue(allFieldResponses, "Sex", "Gender", "M/F"),
                ["YOB"] = participant.User.DateOfBirth?.Year.ToString() ?? "-",
                ["Age (yrs)"] = age?.ToString() ?? "-",
                ["Workplace"] = GetFieldValue(allFieldResponses, "Workplace", "Work Place", "Company"),
                ["Lifestyle Risk"] = GetFieldValue(allFieldResponses, "Lifestyle Risk"),
                ["Sys BP (mmHg)"] = GetFieldValue(allFieldResponses, "Sys BP (mmHg)", "Systolic BP", "SBP", "Systolic Blood Pressure"),
                ["Dia BP (mmHg)"] = GetFieldValue(allFieldResponses, "Dia BP (mmHg)", "Diastolic BP", "DBP", "Diastolic Blood Pressure"),
                ["BP Risk"] = GetFieldValue(allFieldResponses, "BP Risk", "Blood Pressure Risk"),
                ["HR"] = GetFieldValue(allFieldResponses, "HR", "Heart Rate", "Pulse"),
                ["Temp"] = GetFieldValue(allFieldResponses, "Temp", "Temperature"),
                ["RBS (mmol/L)"] = GetFieldValue(allFieldResponses, "RBS (mmol/L)", "RBS", "Random Blood Sugar"),
                ["Diabetes Mellitus Risk"] = GetFieldValue(allFieldResponses, "Diabetes Mellitus Risk", "Diabetes Risk"),
                ["Height (m)"] = GetFieldValue(allFieldResponses, "Height (m)", "Height"),
                ["Weight (kg)"] = GetFieldValue(allFieldResponses, "Weight (kg)", "Weight"),
                ["BMI (kg/m2)"] = GetFieldValue(allFieldResponses, "BMI (kg/m2)", "BMI"),
                ["BMI Risk"] = GetFieldValue(allFieldResponses, "BMI Risk"),
                ["SPO2"] = GetFieldValue(allFieldResponses, "SPO2", "Oxygen Saturation"),
                ["RS"] = GetFieldValue(allFieldResponses, "RS", "Respiratory System"),
                ["CVS"] = GetFieldValue(allFieldResponses, "CVS", "Cardiovascular System"),
                ["MSS"] = GetFieldValue(allFieldResponses, "MSS", "Musculoskeletal System"),
                ["CNS"] = GetFieldValue(allFieldResponses, "CNS", "Central Nervous System"),
                ["Mental health screen"] = GetFieldValue(allFieldResponses, "Mental health screen", "Mental Health"),
                ["Skin"] = GetFieldValue(allFieldResponses, "Skin"),
                ["Breast"] = GetFieldValue(allFieldResponses, "Breast"),
                ["Pap"] = GetFieldValue(allFieldResponses, "Pap", "Pap Smear"),
                ["ECG"] = GetFieldValue(allFieldResponses, "ECG"),
                ["Visual acuity"] = GetFieldValue(allFieldResponses, "Visual acuity", "Vision"),
                ["Colour vision"] = GetFieldValue(allFieldResponses, "Colour vision", "Color Vision"),
                ["Audiometry"] = GetFieldValue(allFieldResponses, "Audiometry", "Hearing"),
                ["Dental"] = GetFieldValue(allFieldResponses, "Dental"),
                ["Body Fat%"] = GetFieldValue(allFieldResponses, "Body Fat%", "Body Fat"),
                ["Body Water%"] = GetFieldValue(allFieldResponses, "Body Water%", "Body Water"),
                ["Muscle Mass (%)"] = GetFieldValue(allFieldResponses, "Muscle Mass (%)", "Muscle Mass"),
                ["Bone Density (%)"] = GetFieldValue(allFieldResponses, "Bone Density (%)", "Bone Density"),
                ["BMR Kcl/day"] = GetFieldValue(allFieldResponses, "BMR Kcl/day", "BMR"),
                ["Metabolic Age"] = GetFieldValue(allFieldResponses, "Metabolic Age"),
                ["Nutrition Review"] = GetFieldValue(allFieldResponses, "Nutrition Review", "Nutrition"),
                ["FHG"] = GetFieldValue(allFieldResponses, "FHG", "Fasting Blood Glucose"),
                ["HbA1c"] = GetFieldValue(allFieldResponses, "HbA1c"),
                ["HbA1c Flags"] = GetFieldValue(allFieldResponses, "HbA1c Flags", "HbA1c Flag"),
                ["TSH"] = GetFieldValue(allFieldResponses, "TSH"),
                ["TSH FLAG"] = GetFieldValue(allFieldResponses, "TSH FLAG", "TSH Flag"),
                ["Urinalysis"] = GetFieldValue(allFieldResponses, "Urinalysis"),
                ["Occult blood"] = GetFieldValue(allFieldResponses, "Occult blood"),
                ["PSA"] = GetFieldValue(allFieldResponses, "PSA"),
                ["PSA Risk"] = GetFieldValue(allFieldResponses, "PSA Risk"),
                ["Cholesterol (mg/dL)"] = GetFieldValue(allFieldResponses, "Cholesterol (mg/dL)", "Cholesterol"),
                ["Lipid Risk"] = GetFieldValue(allFieldResponses, "Lipid Risk"),
                ["HDL (mg/dL)"] = GetFieldValue(allFieldResponses, "HDL (mg/dL)", "HDL"),
                ["HDL Risk"] = GetFieldValue(allFieldResponses, "HDL Risk"),
                ["TG (mg/dL)"] = GetFieldValue(allFieldResponses, "TG (mg/dL)", "TG", "Triglycerides"),
                ["TG Risk"] = GetFieldValue(allFieldResponses, "TG Risk", "Triglycerides Risk"),
                ["LDL (mg/dL)"] = GetFieldValue(allFieldResponses, "LDL (mg/dL)", "LDL"),
                ["LDL Risk"] = GetFieldValue(allFieldResponses, "LDL Risk"),
                ["Creatinine (mg/dL)"] = GetFieldValue(allFieldResponses, "Creatinine (mg/dL)", "Creatinine"),
                ["Creat flag"] = GetFieldValue(allFieldResponses, "Creat flag", "Creatinine Flag"),
                ["GGT (u/L)"] = GetFieldValue(allFieldResponses, "GGT (u/L)", "GGT"),
                ["GGT flag"] = GetFieldValue(allFieldResponses, "GGT flag", "GGT Flag"),
                ["Pertinent History"] = GetFieldValue(allFieldResponses, "Pertinent History", "Pertinent History Findings"),
                ["Clinical findings"] = GetFieldValue(allFieldResponses, "Clinical findings", "Pertinent Clinical Findings"),
                ["Conclusion"] = GetFieldValue(allFieldResponses, "Conclusion"),
                ["Recommendation"] = GetFieldValue(allFieldResponses, "Recommendation"),
                ["Specific Instructions"] = GetFieldValue(allFieldResponses, "Specific Instructions", "Instructions"),
                ["CDMP"] = GetFieldValue(allFieldResponses, "CDMP")
            };

            // Write data to Excel row
            //for (int i = 0; i < headers.Length; i++)
            //{
            //    var value = rowData.TryGetValue(headers[i], out var cellValue) ? cellValue : "-";
            //    worksheet.Cell(currentRow, i + 1).Value = value;
            //    worksheet.Cell(currentRow, i + 1).DataType = XLDataType.Text;
            //}

            for (int i = 0; i < headers.Length; i++)
            {
                var value = rowData.TryGetValue(headers[i], out var cellValue) ? cellValue : "-";
                worksheet.Cell(currentRow, i + 1).SetValue($"'{value}"); // Adding single quote forces text format
            }

            currentRow++;
        }

        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents();

        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return (stream.ToArray(), camp.Name, camp.Organization?.BusinessName ?? "Unknown_Organization");
    }

    private static string GetFieldValue(Dictionary<string, string> fieldResponses, params string[] possibleLabels)
    {
        foreach (var label in possibleLabels)
        {
            if (fieldResponses.TryGetValue(label, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        return "-";
    }

    public async Task<(byte[] ExcelData, string CampName, string OrganizationName)> ExportCampDataToExcelSheetAsync(Guid campId, CancellationToken ct = default)
    {
        // Verify camp exists and get camp details with organization
        var camp = await _healthCampRepository.GetByIdAsync(campId);
        if (camp == null)
            throw new NotFoundException($"Health camp with ID {campId} not found.");

        // Get all responses for this camp with participant and field details
        var responses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(campId, ct);

        if (!responses.Any())
            throw new NotFoundException("No intake form responses found for this camp.");

        // Get all unique fields from all forms used in this camp
        var allFields = responses
            .SelectMany(r => r.FieldResponses)
            .Select(fr => fr.Field)
            .GroupBy(f => f.Id)
            .Select(g => g.First())
            .OrderBy(f => f.Section?.Name ?? "")
            .ThenBy(f => f.Order)
            .ToList();

        // Group responses by participant
        var participantResponses = responses
            .GroupBy(r => r.PatientId)
            .OrderBy(g => g.First().Patient?.User?.FirstName ?? "")
            .ThenBy(g => g.First().Patient?.User?.LastName ?? "")
            .ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Camp Data Export");

        // Create dynamic headers based on actual fields in the system
        var headers = new List<string> { "Participant Name", "Email", "Phone" };

        // Add field labels as headers
        foreach (var field in allFields)
        {
            var sectionPrefix = !string.IsNullOrEmpty(field.Section?.Name) ? $"{field.Section.Name} - " : "";
            headers.Add($"{sectionPrefix}{field.Label}");
        }

        // Set headers in Excel
        for (int i = 0; i < headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Populate data rows
        int currentRow = 2;
        foreach (var participantGroup in participantResponses)
        {
            var participant = participantGroup.First().Patient;
            if (participant?.User == null) continue;

            // Basic participant info
            worksheet.Cell(currentRow, 1).Value = participant.User.FullName ?? "";
            worksheet.Cell(currentRow, 2).Value = participant.User.Email ?? "";
            worksheet.Cell(currentRow, 3).Value = participant.User.Phone ?? "";

            // Create a lookup for all field responses for this participant
            var fieldResponseLookup = participantGroup
                .SelectMany(r => r.FieldResponses)
                .GroupBy(fr => fr.FieldId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First()); // Get latest response if multiple

            // Fill field values
            int columnIndex = 4; // Start after basic info columns
            foreach (var field in allFields)
            {
                string value = "";
                if (fieldResponseLookup.TryGetValue(field.Id, out var fieldResponse))
                {
                    value = fieldResponse.Value ?? "";

                    // Handle different field types for better display
                    value = field.FieldType?.ToLowerInvariant() switch
                    {
                        "checkbox" => value == "true" ? "Yes" : value == "false" ? "No" : value,
                        "date" => DateTime.TryParse(value, out var date) ? date.ToString("yyyy-MM-dd") : value,
                        "number" => decimal.TryParse(value, out var number) ? number.ToString("0.##") : value,
                        _ => value
                    };
                }

                worksheet.Cell(currentRow, columnIndex).Value = value;
                columnIndex++;
            }

            currentRow++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Add some formatting
        var headerRange = worksheet.Range(1, 1, 1, headers.Count);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Add data borders
        if (currentRow > 2)
        {
            var dataRange = worksheet.Range(2, 1, currentRow - 1, headers.Count);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
        }

        // Add summary information
        worksheet.Cell(currentRow + 2, 1).Value = "Export Summary:";
        worksheet.Cell(currentRow + 2, 1).Style.Font.Bold = true;
        worksheet.Cell(currentRow + 3, 1).Value = $"Camp: {camp.Name}";
        worksheet.Cell(currentRow + 4, 1).Value = $"Organization: {camp.Organization?.BusinessName ?? "N/A"}";
        worksheet.Cell(currentRow + 5, 1).Value = $"Total Participants: {participantResponses.Count}";
        worksheet.Cell(currentRow + 6, 1).Value = $"Total Fields: {allFields.Count}";
        worksheet.Cell(currentRow + 7, 1).Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return (stream.ToArray(), camp.Name, camp.Organization?.BusinessName ?? "Unknown_Organization");
    }
}
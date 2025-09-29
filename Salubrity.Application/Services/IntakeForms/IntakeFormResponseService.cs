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
        IIntakeFormRepository intakeFormRepository
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

    public async Task<IntakeFormResponseExportDto> ExportCampResponsesToExcelAsync(Guid campId, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting Excel export for camp {CampId} intake form responses", campId);

        // Get all responses for this camp
        var responses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailsAsync(campId, ct);

        if (!responses.Any())
        {
            _logger.LogWarning("No intake form responses found for camp {CampId}", campId);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Health Camp Data");

        // Define the exact column headers as per your template
        var headers = new List<string>
        {
            "Unnamed: 0",
            "Name",
            "Sex",
            "YOB",
            "Age (yrs)",
            "Workplace",
            "Lifestyle Risk",
            "Sys BP (mmHg)",
            "Dia BP (mmHg)",
            "BP Risk",
            "HR",
            "Temp",
            "RBS (mmol/L)",
            "Diabetes Mellitus Risk",
            "Height (m)",
            "Weight (kg)",
            "BMI (kg/m2)",
            "BMI Risk",
            "SPO2",
            "RS",
            "CVS",
            "MSS",
            "CNS",
            "Mental health screen",
            "Skin",
            "Breast",
            "Pap",
            "ECG",
            "Visual acuity",
            "Colour vision",
            "Audiometry",
            "Dental",
            "Body Fat%",
            "Body Water%",
            "Muscle Mass (%)",
            "Bone Density (%)",
            "BMR Kcl/day",
            "Metabolic Age",
            "Nutrition Review",
            "FHG",
            "HbA1c",
            "HbA1c Flags",
            "TSH",
            "TSH FLAG",
            "Urinalysis",
            "Occult blood",
            "PSA",
            "PSA Risk",
            "Cholesterol (mg/dL)",
            "Lipid Risk",
            "HDL (mg/dL)",
            "HDL Risk",
            "TG (mg/dL)",
            "TG Risk",
            "LDL (mg/dL)",
            "LDL Risk",
            "Creatinine (mg/dL)",
            "Creat flag",
            "GGT (u/L)",
            "GGT flag",
            "Pertinent History",
            "Clinical findings",
            "Conclusion",
            "Recommendation",
            "Specific Instructions",
            "CDMP"
        };

        // Set headers
        for (int i = 0; i < headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            worksheet.Cell(1, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Group responses by participant to consolidate all their data into one row
        var participantResponses = responses
            .GroupBy(r => r.PatientId)
            .ToList();

        _logger.LogInformation("Found {ParticipantCount} participants with responses for camp {CampId}",
            participantResponses.Count, campId);

        // Populate data rows
        int currentRow = 2;
        foreach (var participantGroup in participantResponses)
        {
            var allFieldResponses = participantGroup
                .SelectMany(r => r.FieldResponses)
                .Where(fr => fr.Field != null)
                .ToList();

            // Get participant info from the first response
            var firstResponse = participantGroup.First();
            var participant = firstResponse.Patient;

            _logger.LogInformation("Processing participant {ParticipantId} with {ResponseCount} field responses",
                participantGroup.Key, allFieldResponses.Count);

            // Create a dictionary for easy field lookup by label
            var fieldValueMap = allFieldResponses
                .Where(fr => !string.IsNullOrEmpty(fr.Value))
                .GroupBy(fr => fr.Field.Label.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

            // Map each column
            for (int colIndex = 0; colIndex < headers.Count; colIndex++)
            {
                string cellValue = "";
                string header = headers[colIndex];

                switch (header)
                {
                    case "Unnamed: 0":
                        cellValue = (currentRow - 1).ToString(); // Row number
                        break;
                    case "Name":
                        cellValue = participant?.PatientNumber ?? "";
                        break;
                    case "Sex":
                        cellValue = GetFieldValue(fieldValueMap, "Sex", "Gender", "M/F");
                        break;
                    case "YOB":
                        cellValue = GetFieldValue(fieldValueMap, "YOB", "Year of Birth", "Birth Year");
                        break;
                    case "Age (yrs)":
                        cellValue = GetFieldValue(fieldValueMap, "Age", "Age (yrs)", "Age in years");
                        break;
                    case "Workplace":
                        cellValue = GetFieldValue(fieldValueMap, "Workplace", "Work place", "Occupation");
                        break;
                    case "Lifestyle Risk":
                        cellValue = GetFieldValue(fieldValueMap, "Lifestyle Risk", "Lifestyle", "Risk Assessment");
                        break;
                    case "Sys BP (mmHg)":
                        cellValue = GetFieldValue(fieldValueMap, "Sys BP (mmHg)", "Systolic BP", "SBP", "Systolic");
                        break;
                    case "Dia BP (mmHg)":
                        cellValue = GetFieldValue(fieldValueMap, "Dia BP (mmHg)", "Diastolic BP", "DBP", "Diastolic");
                        break;
                    case "BP Risk":
                        cellValue = GetFieldValue(fieldValueMap, "BP Risk", "Blood Pressure Risk", "Hypertension Risk");
                        break;
                    case "HR":
                        cellValue = GetFieldValue(fieldValueMap, "HR", "Heart Rate", "Pulse");
                        break;
                    case "Temp":
                        cellValue = GetFieldValue(fieldValueMap, "Temp", "Temperature", "Body Temperature");
                        break;
                    case "RBS (mmol/L)":
                        cellValue = GetFieldValue(fieldValueMap, "RBS (mmol/L)", "RBS", "Random Blood Sugar", "Blood Sugar");
                        break;
                    case "Diabetes Mellitus Risk":
                        cellValue = GetFieldValue(fieldValueMap, "Diabetes Mellitus Risk", "Diabetes Risk", "DM Risk");
                        break;
                    case "Height (m)":
                        cellValue = GetFieldValue(fieldValueMap, "Height (m)", "Height", "Height in meters");
                        break;
                    case "Weight (kg)":
                        cellValue = GetFieldValue(fieldValueMap, "Weight (kg)", "Weight", "Weight in kg");
                        break;
                    case "BMI (kg/m2)":
                        cellValue = GetFieldValue(fieldValueMap, "BMI (kg/m2)", "BMI", "Body Mass Index");
                        break;
                    case "BMI Risk":
                        cellValue = GetFieldValue(fieldValueMap, "BMI Risk", "BMI Category", "Weight Status");
                        break;
                    case "SPO2":
                        cellValue = GetFieldValue(fieldValueMap, "SPO2", "Oxygen Saturation", "O2 Saturation");
                        break;
                    case "RS":
                        cellValue = GetFieldValue(fieldValueMap, "RS", "Respiratory System", "Respiratory");
                        break;
                    case "CVS":
                        cellValue = GetFieldValue(fieldValueMap, "CVS", "Cardiovascular System", "Cardiovascular");
                        break;
                    case "MSS":
                        cellValue = GetFieldValue(fieldValueMap, "MSS", "Musculoskeletal System", "Musculoskeletal");
                        break;
                    case "CNS":
                        cellValue = GetFieldValue(fieldValueMap, "CNS", "Central Nervous System", "Neurological");
                        break;
                    case "Mental health screen":
                        cellValue = GetFieldValue(fieldValueMap, "Mental health screen", "Mental Health", "Psychological");
                        break;
                    case "Skin":
                        cellValue = GetFieldValue(fieldValueMap, "Skin", "Skin Examination", "Dermatological");
                        break;
                    case "Breast":
                        cellValue = GetFieldValue(fieldValueMap, "Breast", "Breast Examination", "Breast Exam");
                        break;
                    case "Pap":
                        cellValue = GetFieldValue(fieldValueMap, "Pap", "Pap Smear", "Cervical Screening");
                        break;
                    case "ECG":
                        cellValue = GetFieldValue(fieldValueMap, "ECG", "Electrocardiogram", "EKG");
                        break;
                    case "Visual acuity":
                        cellValue = GetFieldValue(fieldValueMap, "Visual acuity", "Vision Test", "Eye Exam");
                        break;
                    case "Colour vision":
                        cellValue = GetFieldValue(fieldValueMap, "Colour vision", "Color Vision", "Color Blindness Test");
                        break;
                    case "Audiometry":
                        cellValue = GetFieldValue(fieldValueMap, "Audiometry", "Hearing Test", "Audio Test");
                        break;
                    case "Dental":
                        cellValue = GetFieldValue(fieldValueMap, "Dental", "Dental Examination", "Oral Health");
                        break;
                    case "Body Fat%":
                        cellValue = GetFieldValue(fieldValueMap, "Body Fat%", "Body Fat", "Fat Percentage");
                        break;
                    case "Body Water%":
                        cellValue = GetFieldValue(fieldValueMap, "Body Water%", "Body Water", "Water Percentage");
                        break;
                    case "Muscle Mass (%)":
                        cellValue = GetFieldValue(fieldValueMap, "Muscle Mass (%)", "Muscle Mass", "Muscle Percentage");
                        break;
                    case "Bone Density (%)":
                        cellValue = GetFieldValue(fieldValueMap, "Bone Density (%)", "Bone Density", "Bone Mass");
                        break;
                    case "BMR Kcl/day":
                        cellValue = GetFieldValue(fieldValueMap, "BMR Kcl/day", "BMR", "Basal Metabolic Rate");
                        break;
                    case "Metabolic Age":
                        cellValue = GetFieldValue(fieldValueMap, "Metabolic Age", "Metabolic Age", "Body Age");
                        break;
                    case "Nutrition Review":
                        cellValue = GetFieldValue(fieldValueMap, "Nutrition Review", "Nutrition", "Dietary Assessment");
                        break;
                    case "FHG":
                        cellValue = GetFieldValue(fieldValueMap, "FHG", "Fasting Blood Glucose", "FBG");
                        break;
                    case "HbA1c":
                        cellValue = GetFieldValue(fieldValueMap, "HbA1c", "Hemoglobin A1c", "Glycated Hemoglobin");
                        break;
                    case "HbA1c Flags":
                        cellValue = GetFieldValue(fieldValueMap, "HbA1c Flags", "HbA1c Flag", "HbA1c Status");
                        break;
                    case "TSH":
                        cellValue = GetFieldValue(fieldValueMap, "TSH", "Thyroid Stimulating Hormone", "Thyroid Test");
                        break;
                    case "TSH FLAG":
                        cellValue = GetFieldValue(fieldValueMap, "TSH FLAG", "TSH Flag", "TSH Status");
                        break;
                    case "Urinalysis":
                        cellValue = GetFieldValue(fieldValueMap, "Urinalysis", "Urine Test", "Urine Analysis");
                        break;
                    case "Occult blood":
                        cellValue = GetFieldValue(fieldValueMap, "Occult blood", "Occult Blood", "Hidden Blood");
                        break;
                    case "PSA":
                        cellValue = GetFieldValue(fieldValueMap, "PSA", "Prostate Specific Antigen", "Prostate Test");
                        break;
                    case "PSA Risk":
                        cellValue = GetFieldValue(fieldValueMap, "PSA Risk", "PSA Flag", "Prostate Risk");
                        break;
                    case "Cholesterol (mg/dL)":
                        cellValue = GetFieldValue(fieldValueMap, "Cholesterol (mg/dL)", "Cholesterol", "Total Cholesterol");
                        break;
                    case "Lipid Risk":
                        cellValue = GetFieldValue(fieldValueMap, "Lipid Risk", "Cholesterol Risk", "Lipid Profile Risk");
                        break;
                    case "HDL (mg/dL)":
                        cellValue = GetFieldValue(fieldValueMap, "HDL (mg/dL)", "HDL", "HDL Cholesterol", "Good Cholesterol");
                        break;
                    case "HDL Risk":
                        cellValue = GetFieldValue(fieldValueMap, "HDL Risk", "HDL Flag", "HDL Status");
                        break;
                    case "TG (mg/dL)":
                        cellValue = GetFieldValue(fieldValueMap, "TG (mg/dL)", "TG", "Triglycerides", "Triglyceride");
                        break;
                    case "TG Risk":
                        cellValue = GetFieldValue(fieldValueMap, "TG Risk", "TG Flag", "Triglyceride Risk");
                        break;
                    case "LDL (mg/dL)":
                        cellValue = GetFieldValue(fieldValueMap, "LDL (mg/dL)", "LDL", "LDL Cholesterol", "Bad Cholesterol");
                        break;
                    case "LDL Risk":
                        cellValue = GetFieldValue(fieldValueMap, "LDL Risk", "LDL Flag", "LDL Status");
                        break;
                    case "Creatinine (mg/dL)":
                        cellValue = GetFieldValue(fieldValueMap, "Creatinine (mg/dL)", "Creatinine", "Serum Creatinine");
                        break;
                    case "Creat flag":
                        cellValue = GetFieldValue(fieldValueMap, "Creat flag", "Creatinine Flag", "Kidney Function");
                        break;
                    case "GGT (u/L)":
                        cellValue = GetFieldValue(fieldValueMap, "GGT (u/L)", "GGT", "Gamma GT", "Liver Enzyme");
                        break;
                    case "GGT flag":
                        cellValue = GetFieldValue(fieldValueMap, "GGT flag", "GGT Flag", "Liver Function");
                        break;
                    case "Pertinent History":
                        cellValue = GetFieldValue(fieldValueMap, "Pertinent History", "Medical History", "Past History", "History");
                        break;
                    case "Clinical findings":
                        cellValue = GetFieldValue(fieldValueMap, "Clinical findings", "Clinical Findings", "Examination Findings", "Physical Findings");
                        break;
                    case "Conclusion":
                        cellValue = GetFieldValue(fieldValueMap, "Conclusion", "Assessment", "Diagnosis", "Summary");
                        break;
                    case "Recommendation":
                        cellValue = GetFieldValue(fieldValueMap, "Recommendation", "Recommendations", "Advice", "Plan");
                        break;
                    case "Specific Instructions":
                        cellValue = GetFieldValue(fieldValueMap, "Specific Instructions", "Instructions", "Special Instructions", "Notes");
                        break;
                    case "CDMP":
                        cellValue = GetFieldValue(fieldValueMap, "CDMP", "Care Plan", "Management Plan");
                        break;
                    default:
                        // Try direct field mapping for any other columns
                        cellValue = GetFieldValue(fieldValueMap, header);
                        break;
                }

                worksheet.Cell(currentRow, colIndex + 1).Value = cellValue;
            }

            currentRow++;
        }

        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents();

        // Add borders to all cells with data
        var dataRange = worksheet.Range(1, 1, currentRow - 1, headers.Count);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Create the Excel file in memory
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"HealthCamp_Data_{campId}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        _logger.LogInformation("Excel export completed for camp {CampId}. Generated {RowCount} rows with {ColumnCount} columns",
            campId, participantResponses.Count, headers.Count);

        return new IntakeFormResponseExportDto
        {
            Content = content,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = fileName
        };
    }

    // Helper method to get field values with multiple possible field names
    private static string GetFieldValue(Dictionary<string, string> fieldValueMap, params string[] possibleFieldNames)
    {
        foreach (var fieldName in possibleFieldNames)
        {
            if (fieldValueMap.TryGetValue(fieldName, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }
        }
        return "";
    }


}

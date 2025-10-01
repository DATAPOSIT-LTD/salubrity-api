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

    //public async Task<(byte[] ExcelData, string CampName, string OrganizationName)> ExportCampDataToExcelAsync(Guid campId, CancellationToken ct = default)
    //{
    //    // Verify camp exists
    //    var camp = await _healthCampRepository.GetByIdAsync(campId);

    //    if (camp == null)
    //        throw new NotFoundException($"Health camp with ID {campId} not found.");

    //    // Get all participants for this specific camp
    //    var participants = await _healthCampRepository.GetParticipantsAsync(campId, null, null, ct);

    //    if (!participants.Any())
    //        throw new NotFoundException("No participants found for this camp.");

    //    // Get all intake form responses for this camp
    //    var responses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(campId, ct);

    //    // Create a comprehensive dataset by combining participants with their responses
    //    var participantData = participants.Select(participant => new
    //    {
    //        Participant = participant,
    //        Patient = participant.Patient,
    //        User = participant.User,
    //        Responses = responses.Where(r => r.PatientId == participant.PatientId).ToList()
    //    }).ToList();

    //    using var workbook = new XLWorkbook();
    //    var worksheet = workbook.Worksheets.Add($"Camp Data - {camp.Name}");

    //    // Define comprehensive column headers
    //    var headers = new[]
    //    {
    //    "Participant ID", "Patient ID", "Name", "Email", "Phone", "Sex", "Date of Birth", "Age (yrs)",
    //    "Workplace", "Lifestyle Risk", "Sys BP (mmHg)", "Dia BP (mmHg)", "BP Risk", "HR", "Temp",
    //    "RBS (mmol/L)", "Diabetes Mellitus Risk", "Height (m)", "Weight (kg)", "BMI (kg/m2)", "BMI Risk",
    //    "SPO2", "RS", "CVS", "MSS", "CNS", "Mental health screen", "Skin", "Breast", "Pap", "ECG",
    //    "Visual acuity", "Colour vision", "Audiometry", "Dental", "Body Fat%", "Body Water%",
    //    "Muscle Mass (%)", "Bone Density (%)", "BMR Kcl/day", "Metabolic Age", "Nutrition Review",
    //    "FHG", "HbA1c", "HbA1c Flags", "TSH", "TSH FLAG", "Urinalysis", "Occult blood", "PSA",
    //    "PSA Risk", "Cholesterol (mg/dL)", "Lipid Risk", "HDL (mg/dL)", "HDL Risk", "TG (mg/dL)",
    //    "TG Risk", "LDL (mg/dL)", "LDL Risk", "Creatinine (mg/dL)", "Creat flag", "GGT (u/L)",
    //    "GGT flag", "Pertinent History", "Clinical findings", "Conclusion", "Recommendation",
    //    "Specific Instructions", "CDMP", "Participation Date", "Status"
    //};

    //    // Add headers to worksheet
    //    for (int i = 0; i < headers.Length; i++)
    //    {
    //        worksheet.Cell(1, i + 1).Value = headers[i];
    //        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
    //        worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
    //    }

    //    // Populate data rows
    //    int row = 2;
    //    foreach (var participantInfo in participantData)
    //    {
    //        var participant = participantInfo.Participant;
    //        var user = participantInfo.User;
    //        var responses = participantInfo.Responses;

    //        // Create a field value lookup from all responses for this participant
    //        var fieldValues = new Dictionary<string, string>();
    //        foreach (var response in responses)
    //        {
    //            foreach (var fieldResponse in response.FieldResponses)
    //            {
    //                var fieldKey = fieldResponse.Field?.Label?.ToLowerInvariant().Trim() ?? "";
    //                if (!string.IsNullOrEmpty(fieldKey) && !fieldValues.ContainsKey(fieldKey))
    //                {
    //                    fieldValues[fieldKey] = fieldResponse.Value ?? "";
    //                }
    //            }
    //        }

    //        // Helper function to get field value by various possible field names
    //        string GetFieldValue(params string[] possibleNames)
    //        {
    //            foreach (var name in possibleNames)
    //            {
    //                var key = name.ToLowerInvariant().Trim();
    //                if (fieldValues.ContainsKey(key))
    //                    return fieldValues[key];
    //            }
    //            return "";
    //        }

    //        // Calculate age if date of birth is available
    //        var ageText = "";
    //        if (user.DateOfBirth.HasValue)
    //        {
    //            var age = DateTime.Now.Year - user.DateOfBirth.Value.Year;
    //            if (DateTime.Now.DayOfYear < user.DateOfBirth.Value.DayOfYear)
    //                age--;
    //            ageText = age.ToString();
    //        }

    //        // Populate row data
    //        var rowData = new object[]
    //        {
    //        participant.Id.ToString(),
    //        participant.PatientId?.ToString() ?? "",
    //        user.FullName ?? "",
    //        user.Email ?? "",
    //        user.Phone ?? "",
    //        GetFieldValue("sex", "gender"),
    //        user.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
    //        ageText,
    //        GetFieldValue("workplace", "company", "employer"),
    //        GetFieldValue("lifestyle risk", "lifestyle"),
    //        GetFieldValue("systolic bp", "sys bp", "systolic blood pressure"),
    //        GetFieldValue("diastolic bp", "dia bp", "diastolic blood pressure"),
    //        GetFieldValue("bp risk", "blood pressure risk"),
    //        GetFieldValue("hr", "heart rate", "pulse"),
    //        GetFieldValue("temp", "temperature"),
    //        GetFieldValue("rbs", "random blood sugar", "blood sugar"),
    //        GetFieldValue("diabetes mellitus risk", "diabetes risk"),
    //        GetFieldValue("height"),
    //        GetFieldValue("weight"),
    //        GetFieldValue("bmi"),
    //        GetFieldValue("bmi risk"),
    //        GetFieldValue("spo2", "oxygen saturation"),
    //        GetFieldValue("rs", "respiratory system"),
    //        GetFieldValue("cvs", "cardiovascular system"),
    //        GetFieldValue("mss", "musculoskeletal system"),
    //        GetFieldValue("cns", "central nervous system"),
    //        GetFieldValue("mental health screen", "mental health"),
    //        GetFieldValue("skin"),
    //        GetFieldValue("breast"),
    //        GetFieldValue("pap", "pap smear"),
    //        GetFieldValue("ecg", "electrocardiogram"),
    //        GetFieldValue("visual acuity", "vision"),
    //        GetFieldValue("colour vision", "color vision"),
    //        GetFieldValue("audiometry", "hearing"),
    //        GetFieldValue("dental"),
    //        GetFieldValue("body fat", "body fat%"),
    //        GetFieldValue("body water", "body water%"),
    //        GetFieldValue("muscle mass"),
    //        GetFieldValue("bone density"),
    //        GetFieldValue("bmr", "basal metabolic rate"),
    //        GetFieldValue("metabolic age"),
    //        GetFieldValue("nutrition review", "nutrition"),
    //        GetFieldValue("fhg", "fasting glucose"),
    //        GetFieldValue("hba1c"),
    //        GetFieldValue("hba1c flags", "hba1c flag"),
    //        GetFieldValue("tsh"),
    //        GetFieldValue("tsh flag"),
    //        GetFieldValue("urinalysis", "urine analysis"),
    //        GetFieldValue("occult blood"),
    //        GetFieldValue("psa"),
    //        GetFieldValue("psa risk"),
    //        GetFieldValue("cholesterol"),
    //        GetFieldValue("lipid risk"),
    //        GetFieldValue("hdl"),
    //        GetFieldValue("hdl risk"),
    //        GetFieldValue("tg", "triglycerides"),
    //        GetFieldValue("tg risk", "triglyceride risk"),
    //        GetFieldValue("ldl"),
    //        GetFieldValue("ldl risk"),
    //        GetFieldValue("creatinine"),
    //        GetFieldValue("creat flag", "creatinine flag"),
    //        GetFieldValue("ggt"),
    //        GetFieldValue("ggt flag"),
    //        GetFieldValue("pertinent history", "medical history"),
    //        GetFieldValue("clinical findings", "findings"),
    //        GetFieldValue("conclusion"),
    //        GetFieldValue("recommendation", "recommendations"),
    //        GetFieldValue("specific instructions", "instructions"),
    //        GetFieldValue("cdmp"),
    //        participant.ParticipatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
    //        participant.ParticipatedAt != null ? "Participated" : "Registered"
    //        };

    //        // Write row data
    //        for (int col = 0; col < rowData.Length; col++)
    //        {
    //            worksheet.Cell(row, col + 1).Value = rowData[col]?.ToString() ?? "";
    //        }

    //        row++;
    //    }

    //    // Auto-fit columns
    //    worksheet.ColumnsUsed().AdjustToContents();

    //    // Save to memory stream
    //    using var stream = new MemoryStream();
    //    workbook.SaveAs(stream);
    //    return (stream.ToArray(), camp.Name, camp.Organization?.BusinessName ?? "Unknown_Organization");
    //}

    public async Task<(byte[] ExcelData, string CampName, string OrganizationName)> ExportCampDataToExcelAsync(Guid campId, CancellationToken ct = default)
    {
        // Verify camp exists
        var camp = await _healthCampRepository.GetByIdAsync(campId);
        if (camp == null)
            throw new NotFoundException($"Health camp with ID {campId} not found.");

        // Get all intake form responses for this specific camp
        var responses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(campId, ct);

        if (!responses.Any())
            throw new NotFoundException("No intake form responses found for this camp.");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add($"Camp Data - {camp.Name}");

        // Define the exact column headers as specified
        var headers = new[]
        {
        "Name", "Sex", "YOB", "Age (yrs)", "Workplace", "Lifestyle Risk", "Sys BP (mmHg)",
        "Dia BP (mmHg)", "BP Risk", "HR", "Temp", "RBS (mmol/L)", "Diabetes Mellitus Risk",
        "Height (m)", "Weight (kg)", "BMI (kg/m2)", "BMI Risk", "SPO2", "RS", "CVS", "MSS",
        "CNS", "Mental health screen", "Skin", "Breast", "Pap", "ECG", "Visual acuity",
        "Colour vision", "Audiometry", "Dental", "Body Fat%", "Body Water%", "Muscle Mass (%)",
        "Bone Density (%)", "BMR Kcl/day", "Metabolic Age", "Nutrition Review", "FHG", "HbA1c",
        "HbA1c Flags", "TSH", "TSH FLAG", "Urinalysis", "Occult blood", "PSA", "PSA Risk",
        "Cholesterol (mg/dL)", "Lipid Risk", "HDL (mg/dL)", "HDL Risk", "TG (mg/dL)", "TG Risk",
        "LDL (mg/dL)", "LDL Risk", "Creatinine (mg/dL)", "Creat flag", "GGT (u/L)", "GGT flag",
        "Pertinent History", "Clinical findings", "Conclusion", "Recommendation",
        "Specific Instructions", "CDMP"
    };

        // Add headers to worksheet
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Group responses by patient to handle multiple responses per patient
        var patientResponses = responses.GroupBy(r => r.PatientId).ToList();

        int row = 2;
        foreach (var patientGroup in patientResponses)
        {
            var patientResponsesList = patientGroup.ToList();
            var firstResponse = patientResponsesList.First();
            var user = firstResponse.Patient?.User;

            if (user == null) continue;

            // Combine all field responses for this patient
            var allFieldResponses = patientResponsesList
                .SelectMany(r => r.FieldResponses)
                .Where(fr => fr.Field != null)
                .ToList();

            // Create field value lookup
            var fieldValues = new Dictionary<string, string>();
            foreach (var fieldResponse in allFieldResponses)
            {
                var fieldKey = fieldResponse.Field.Label?.ToLowerInvariant().Trim() ?? "";
                if (!string.IsNullOrEmpty(fieldKey) && !fieldValues.ContainsKey(fieldKey))
                {
                    fieldValues[fieldKey] = fieldResponse.Value ?? "";
                }
            }

            // Helper function to get field value by various possible field names
            string GetFieldValue(params string[] possibleNames)
            {
                foreach (var name in possibleNames)
                {
                    var key = name.ToLowerInvariant().Trim();
                    if (fieldValues.ContainsKey(key))
                        return fieldValues[key];
                }
                return "";
            }

            // Calculate age and year of birth
            var yob = "";
            var ageText = "";
            if (user.DateOfBirth.HasValue)
            {
                yob = user.DateOfBirth.Value.Year.ToString();
                var age = DateTime.Now.Year - user.DateOfBirth.Value.Year;
                if (DateTime.Now.DayOfYear < user.DateOfBirth.Value.DayOfYear)
                    age--;
                ageText = age.ToString();
            }

            // Populate row data with exact column mapping
            var rowData = new object[]
            {
            user.FullName ?? "",
            GetFieldValue("sex", "gender"),
            yob,
            ageText,
            GetFieldValue("workplace", "company", "employer"),
            GetFieldValue("lifestyle risk", "lifestyle"),
            GetFieldValue("systolic bp", "sys bp", "systolic blood pressure"),
            GetFieldValue("diastolic bp", "dia bp", "diastolic blood pressure"),
            GetFieldValue("bp risk", "blood pressure risk"),
            GetFieldValue("hr", "heart rate", "pulse"),
            GetFieldValue("temp", "temperature"),
            GetFieldValue("rbs", "random blood sugar", "blood sugar"),
            GetFieldValue("diabetes mellitus risk", "diabetes risk"),
            GetFieldValue("height"),
            GetFieldValue("weight"),
            GetFieldValue("bmi"),
            GetFieldValue("bmi risk"),
            GetFieldValue("spo2", "oxygen saturation"),
            GetFieldValue("rs", "respiratory system"),
            GetFieldValue("cvs", "cardiovascular system"),
            GetFieldValue("mss", "musculoskeletal system"),
            GetFieldValue("cns", "central nervous system"),
            GetFieldValue("mental health screen", "mental health"),
            GetFieldValue("skin"),
            GetFieldValue("breast"),
            GetFieldValue("pap", "pap smear"),
            GetFieldValue("ecg", "electrocardiogram"),
            GetFieldValue("visual acuity", "vision"),
            GetFieldValue("colour vision", "color vision"),
            GetFieldValue("audiometry", "hearing"),
            GetFieldValue("dental"),
            GetFieldValue("body fat", "body fat%"),
            GetFieldValue("body water", "body water%"),
            GetFieldValue("muscle mass"),
            GetFieldValue("bone density"),
            GetFieldValue("bmr", "basal metabolic rate"),
            GetFieldValue("metabolic age"),
            GetFieldValue("nutrition review", "nutrition"),
            GetFieldValue("fhg", "fasting glucose"),
            GetFieldValue("hba1c"),
            GetFieldValue("hba1c flags", "hba1c flag"),
            GetFieldValue("tsh"),
            GetFieldValue("tsh flag"),
            GetFieldValue("urinalysis", "urine analysis"),
            GetFieldValue("occult blood"),
            GetFieldValue("psa"),
            GetFieldValue("psa risk"),
            GetFieldValue("cholesterol"),
            GetFieldValue("lipid risk"),
            GetFieldValue("hdl"),
            GetFieldValue("hdl risk"),
            GetFieldValue("tg", "triglycerides"),
            GetFieldValue("tg risk", "triglyceride risk"),
            GetFieldValue("ldl"),
            GetFieldValue("ldl risk"),
            GetFieldValue("creatinine"),
            GetFieldValue("creat flag", "creatinine flag"),
            GetFieldValue("ggt"),
            GetFieldValue("ggt flag"),
            GetFieldValue("pertinent history", "medical history"),
            GetFieldValue("clinical findings", "findings"),
            GetFieldValue("conclusion"),
            GetFieldValue("recommendation", "recommendations"),
            GetFieldValue("specific instructions", "instructions"),
            GetFieldValue("cdmp")
            };

            // Write row data
            for (int col = 0; col < rowData.Length; col++)
            {
                worksheet.Cell(row, col + 1).Value = rowData[col]?.ToString() ?? "";
            }

            row++;
        }

        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents();

        // Save to memory stream
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
}

using System.Data;
using System.Text;
using OfficeOpenXml;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.Patients;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Shared.Exceptions;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Microsoft.Extensions.Logging;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Services.IntakeForms;

public class BulkLabUploadService : IBulkLabUploadService
{
    private readonly IFormFieldMappingRepository _mappingRepo;
    private readonly IIntakeFormResponseService _formResponseService;
    private readonly IPatientRepository _patientRepo;
    private readonly IIntakeFormRepository _formRepo;
    private readonly ISubcontractorRepository _subcontractorRepo;
    private readonly IHealthCampServiceAssignmentRepository _assignmentRepo;
    private readonly IPackageReferenceResolver _packageResolver;
    private readonly ILogger<BulkLabUploadService> _logger;

    private readonly IIntakeFormRepository _intakeFormRepository;
    private readonly IServiceRepository _serviceRepo;
    private readonly IServiceCategoryRepository _categoryRepo;
    private readonly IServiceSubcategoryRepository _subcategoryRepo;
    private readonly IHealthCampParticipantRepository _healthCampParticipantRepository;
    private readonly ICampQueueRepository _stationCheckInRepository;

    public BulkLabUploadService(
        IFormFieldMappingRepository mappingRepo,
        IIntakeFormResponseService formResponseService,
        IPatientRepository patientRepo,
        IIntakeFormRepository formRepo,
        ISubcontractorRepository subcontractorRepo,
        IHealthCampServiceAssignmentRepository assignmentRepo,
        IPackageReferenceResolver packageResolver,
        IServiceRepository serviceRepo,
        IServiceCategoryRepository categoryRepo,
        IServiceSubcategoryRepository subcategoryRepo,
        ILogger<BulkLabUploadService> logger,
        IIntakeFormRepository intakeFormRepository,
        IHealthCampParticipantRepository healthCampParticipantRepository,
        ICampQueueRepository stationCheckInRepository
    )
    {
        _mappingRepo = mappingRepo;
        _formResponseService = formResponseService;
        _patientRepo = patientRepo;
        _formRepo = formRepo;
        _subcontractorRepo = subcontractorRepo;
        _assignmentRepo = assignmentRepo;
        _packageResolver = packageResolver;

        _serviceRepo = serviceRepo;
        _categoryRepo = categoryRepo;
        _subcategoryRepo = subcategoryRepo;
        _logger = logger;
        _intakeFormRepository = intakeFormRepository;
        _healthCampParticipantRepository = healthCampParticipantRepository;
        _stationCheckInRepository = stationCheckInRepository;
    }

    public async Task<BulkUploadResultDto> UploadExcelAsync(CreateBulkLabUploadDto dto, CancellationToken ct = default)
    {


        ExcelPackage.License.SetNonCommercialOrganization("Salubrity");

        var result = new BulkUploadResultDto();

        using var package = new ExcelPackage(dto.ExcelFile.OpenReadStream());
        foreach (var sheet in package.Workbook.Worksheets)
        {
            _logger.LogInformation("🗂 Starting processing of sheet: {SheetName}", sheet.Name);

            // 1. Load form version including sections + fields
            var formVersion = await _intakeFormRepository.GetActiveVersionWithFieldsByServiceNameAsync(sheet.Name, ct);

            if (formVersion == null)
            {
                _logger.LogError("❌ No IntakeFormVersion found for sheet: {SheetName}", sheet.Name);
                result.Errors.Add(new BulkUploadError
                {
                    Row = 0,
                    Message = $"No Intake Form configured for sheet: {sheet.Name}"
                });
                continue;
            }

            // 2. Flatten all fields
            var availableFields = formVersion.Sections.SelectMany(s => s.Fields).ToList();

            // 3. Map header → fieldId
            var headerToFieldId = availableFields
                .ToDictionary(f => f.Label.Trim(), f => f.Id, StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation("📄 Sheet={SheetName}, Rows={RowCount}, Headers={HeaderCount}",
                sheet.Name, sheet.Dimension.End.Row, headerToFieldId.Count);

            // 4. Process each row
            for (int rowIndex = 2; rowIndex <= sheet.Dimension.End.Row; rowIndex++)
            {
                string? patientNumber = null;
                Guid? patientId = null;
                Guid? participantId = null;
                Guid? checkInId = null;
                Guid? assignmentId = null;

                try
                {
                    patientNumber = sheet.Cells[rowIndex, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(patientNumber))
                        throw new NotFoundException("Missing patient number");

                    _logger.LogInformation("👤 Row={RowIndex}, PatientNumber={PatientNumber}", rowIndex, patientNumber);

                    patientId = await _patientRepo.GetPatientIdByPatientNumberAsync(patientNumber, ct)
                                 ?? throw new NotFoundException($"Patient not found: {patientNumber}");

                    participantId = await _healthCampParticipantRepository.GetParticipantIdByPatientIdAsync(patientId.Value, ct)
                                    ?? throw new NotFoundException($"Participant not found for patient {patientNumber}");

                    var checkIn = await _stationCheckInRepository.GetLatestForParticipantAsync(participantId.Value, ct);
                    HealthCampServiceAssignment? assignment = null;

                    if (checkIn != null && checkIn.HealthCampServiceAssignmentId != Guid.Empty)
                    {
                        checkInId = checkIn.Id;
                        assignment = await _assignmentRepo.GetByIdAsync(checkIn.HealthCampServiceAssignmentId, ct);

                        if (assignment != null)
                        {
                            assignmentId = assignment.Id;
                            _logger.LogInformation("🔗 Resolved assignment: AssignmentId={AssignmentId}, Type={AssignmentType}, FromCheckIn={CheckInId}",
                                assignment.AssignmentId, assignment.AssignmentType, checkInId);

                            var resolvedService = await _packageResolver.ResolveServiceAsync(
                                assignment.AssignmentId, assignment.AssignmentType);

                            if (resolvedService == null)
                            {
                                _logger.LogError("❌ Could not resolve service from assignment. AssignmentId={AssignmentId}, Type={AssignmentType}, Participant={ParticipantId}, Patient={PatientNumber}",
                                    assignment.AssignmentId, assignment.AssignmentType, participantId, patientNumber);

                                throw new ValidationException([$"Assignment {assignment.Id} points to missing service."]);
                            }

                            _logger.LogInformation("✅ Resolved Service: {ServiceName} (ID={ServiceId})", resolvedService.Name, resolvedService.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No valid check-in or assignment for participant: {ParticipantId}, Patient: {PatientNumber}", participantId, patientNumber);
                    }

                    // Build responses
                    var fieldResponses = new List<CreateIntakeFormFieldResponseDto>();
                    for (int colIndex = 3; colIndex <= sheet.Dimension.End.Column; colIndex++)
                    {
                        var header = sheet.Cells[1, colIndex].Text?.Trim();
                        if (string.IsNullOrEmpty(header)) continue;

                        if (!headerToFieldId.TryGetValue(header, out var fieldId))
                        {
                            _logger.LogWarning("⚠️ No matching field for header: '{Header}' on sheet {Sheet}", header, sheet.Name);
                            continue;
                        }

                        var value = sheet.Cells[rowIndex, colIndex].Text?.Trim();
                        _logger.LogWarning("⚠️ Header - Value: '{Header}' on sheet {Value}", header, value);

                        if (!string.IsNullOrEmpty(value))
                        {
                            fieldResponses.Add(new CreateIntakeFormFieldResponseDto
                            {
                                FieldId = fieldId,
                                Value = value
                            });
                        }
                    }

                    if (fieldResponses.Count == 0)
                    {
                        _logger.LogInformation("🚫 Row {RowIndex} skipped: No responses for patient {PatientNumber}", rowIndex, patientNumber);
                        continue;
                    }

                    var service = await _categoryRepo.GetByNameAsync(sheet.Name);

                    if (service == null)
                        continue;

                    if (fieldResponses.Count == 0)
                    {
                        _logger.LogInformation("🚫 Row {RowIndex} skipped: No valid service for {sheet.Name}", rowIndex, patientNumber);
                        continue;
                    }

                    var formDto = new CreateIntakeFormResponseDto
                    {
                        PatientId = participantId!.Value,
                        IntakeFormVersionId = formVersion.Id,
                        FieldResponses = fieldResponses,
                        ResponseStatusId = Guid.Parse("c40c5a53-55bd-4c58-81dd-fafc54db0ac7"),
                        // HealthCampServiceAssignmentId = assignment?.Id
                        ServiceId = service.Id

                    };

                    _logger.LogInformation("📤 Submitting FormResponse for Patient={PatientNumber}, FormId={FormId}, ParticipantId={ParticipantId}, AssignmentId={AssignmentId}",
                        patientNumber, formVersion.Id, participantId, assignment?.Id);

                    await _formResponseService.SubmitResponseAsync(formDto, dto.SubmittedByUserId, ct);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailureCount++;

                    result.Errors.Add(new BulkUploadError
                    {
                        Row = rowIndex,
                        Message = ex.Message
                    });

                    _logger.LogError(ex, "❌ Failed processing Row={RowIndex}, Patient={PatientNumber}, ParticipantId={ParticipantId}, AssignmentId={AssignmentId}, CheckInId={CheckInId}",
                        rowIndex, patientNumber ?? "?", participantId?.ToString() ?? "?", assignmentId?.ToString() ?? "?", checkInId?.ToString() ?? "?");
                }
            }
        }

        return result;
    }



    public async Task<Stream> GenerateLabTemplateForCampAsync(Guid userId, Guid campId, CancellationToken ct = default)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Salubrity");

        var stream = new MemoryStream();
        using var package = new ExcelPackage(stream);

        // Get subcontractor
        var subcontractor = await _subcontractorRepo.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("You are not linked to any provider profile.");

        // Get assignments for this subcontractor
        var assignments = await _assignmentRepo.GetBySubcontractorIdAsync(subcontractor.Id, ct);

        var scopedAssignments = assignments
            .Where(a => a.HealthCampId == campId && a.HealthCamp != null && !a.HealthCamp.IsDeleted)
            .ToList();

        if (scopedAssignments.Count == 0)
            throw new NotFoundException("You are not assigned to the selected health camp.");

        // Collect intake forms & map to service/category/subcategory names
        var intakeFormIds = new HashSet<Guid>();
        var intakeFormEntries = new Dictionary<Guid, string>();

        foreach (var assignment in scopedAssignments)
        {
            ct.ThrowIfCancellationRequested();

            switch (assignment.AssignmentType)
            {
                case PackageItemType.Service:
                    var service = await _serviceRepo.GetByIdAsync(assignment.AssignmentId, ct);
                    if (service?.IntakeFormId != null)
                    {
                        var formId = service.IntakeFormId.Value;
                        intakeFormIds.Add(formId);
                        intakeFormEntries[formId] = service.Name;
                    }
                    break;

                case PackageItemType.ServiceCategory:
                    var category = await _categoryRepo.GetByIdAsync(assignment.AssignmentId);
                    if (category?.IntakeFormId != null)
                    {
                        var formId = category.IntakeFormId.Value;
                        intakeFormIds.Add(formId);
                        intakeFormEntries[formId] = category.Name;
                    }
                    break;

                case PackageItemType.ServiceSubcategory:
                    var subcategory = await _subcategoryRepo.GetByIdAsync(assignment.AssignmentId);
                    if (subcategory?.IntakeFormId != null)
                    {
                        var formId = subcategory.IntakeFormId.Value;
                        intakeFormIds.Add(formId);
                        intakeFormEntries[formId] = subcategory.Name;
                    }
                    break;
            }
        }

        if (intakeFormIds.Count == 0)
            throw new NotFoundException("No intake forms assigned to your services in this camp.");

        // Only include forms that are flagged as Lab Forms
        var formsToInclude = await _formRepo.GetLabFormsByIdsAsync(intakeFormIds, ct);

        if (formsToInclude.Count == 0)
            throw new NotFoundException("No lab forms found for your assigned services.");

        // Get all patients in the camp
        var patients = await _patientRepo.GetPatientsByCampViaUserAsync(campId, ct);

        // Track used sheet names to avoid duplicates
        var usedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var form in formsToInclude)
        {
            ct.ThrowIfCancellationRequested();

            // Resolve sheet name
            string baseName = intakeFormEntries.TryGetValue(form.Id, out var serviceName)
                ? serviceName
                : form.Name;

            // Ensure uniqueness (append counter if duplicate)
            string sheetName = baseName;
            int counter = 2;
            while (usedSheetNames.Contains(sheetName))
            {
                sheetName = $"{baseName} ({counter})";
                counter++;
            }
            usedSheetNames.Add(sheetName);

            // Create worksheet
            var sheet = package.Workbook.Worksheets.Add(sheetName);

            var headers = new List<string> { "Patient Number", "Patient Full Name" };

            foreach (var section in form.Sections.OrderBy(s => s.Order))
            {
                headers.AddRange(
                    section.Fields
                        .OrderBy(f => f.Order)
                        .Select(f => f.Label)
                );
            }

            // Write headers
            for (int col = 0; col < headers.Count; col++)
                sheet.Cells[1, col + 1].Value = headers[col];

            sheet.View.FreezePanes(2, 1);

            // Write patient rows
            int rowIndex = 2;
            foreach (var patient in patients)
            {
                ct.ThrowIfCancellationRequested();
                if (patient.User == null) continue;

                var fullName = string.Join(" ", new[]
                {
                patient.User.FirstName,
                patient.User.MiddleName,
                patient.User.LastName
            }.Where(n => !string.IsNullOrWhiteSpace(n)));

                sheet.Cells[rowIndex, 1].Value = patient.PatientNumber;
                sheet.Cells[rowIndex, 2].Value = fullName;

                for (int col = 3; col <= headers.Count; col++)
                    sheet.Cells[rowIndex, col].Value = string.Empty;

                rowIndex++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        package.Save();
        stream.Position = 0;
        return stream;
    }




}

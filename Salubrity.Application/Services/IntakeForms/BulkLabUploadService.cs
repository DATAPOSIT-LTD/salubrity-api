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
    // NEW
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
            // 1. Load form version including sections + fields
            var formVersion = await _intakeFormRepository.GetVersionWithFieldsAsync(sheet.Name, ct);
            if (formVersion == null)
            {
                result.Errors.Add(new BulkUploadError
                {
                    Row = 0,
                    Message = $"No Intake Form configured for sheet: {sheet.Name}"
                });
                continue;
            }

            // 2. Flatten all fields in this version
            var availableFields = formVersion.Sections
                .SelectMany(s => s.Fields)
                .ToList();

            // 3. Build header → fieldId dictionary
            var headerToFieldId = availableFields
                .ToDictionary(f => f.Label.Trim(), f => f.Id, StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation("📄 Processing Sheet={SheetName}, Rows={RowCount}, Headers={HeaderCount}",
                sheet.Name, sheet.Dimension.End.Row, headerToFieldId.Count);

            // 4. Process each data row
            for (int rowIndex = 2; rowIndex <= sheet.Dimension.End.Row; rowIndex++)
            {
                try
                {
                    var patientNumber = sheet.Cells[rowIndex, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(patientNumber))
                        throw new NotFoundException("Missing patient number");

                    var patientId = await _patientRepo.GetPatientIdByPatientNumberAsync(patientNumber, ct)
                                    ?? throw new NotFoundException($"Patient not found: {patientNumber}");

                    var participantId = await _healthCampParticipantRepository.GetParticipantIdByPatientIdAsync(patientId, ct)
                         ?? throw new NotFoundException($"Participant not found for patient {patientNumber}");

                    // 🧠 Resolve HealthCampServiceAssignmentId from active check-in
                    var checkIn = await _stationCheckInRepository.GetActiveForParticipantAsync(participantId, null, ct)
                                  ?? throw new ValidationException([$"No active station check-in found for participant {participantId}."]);

                    if (checkIn.HealthCampServiceAssignmentId == Guid.Empty)
                        throw new ValidationException([$"Check-in for participant {participantId} has no linked service assignment."]);

                    var fieldResponses = new List<CreateIntakeFormFieldResponseDto>();

                    // Loop through columns starting from C (index 3)
                    for (int colIndex = 3; colIndex <= sheet.Dimension.End.Column; colIndex++)
                    {
                        var header = sheet.Cells[1, colIndex].Text?.Trim();
                        if (string.IsNullOrEmpty(header)) continue;

                        if (!headerToFieldId.TryGetValue(header, out var fieldId))
                        {
                            _logger.LogWarning("⚠️ No IntakeFormField found for header '{Header}' on sheet {Sheet}", header, sheet.Name);
                            continue;
                        }

                        var value = sheet.Cells[rowIndex, colIndex].Text?.Trim();
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
                        _logger.LogInformation("Row {Row} skipped: No responses for patient {PatientNumber}", rowIndex, patientNumber);
                        continue;
                    }

                    var formDto = new CreateIntakeFormResponseDto
                    {
                        PatientId = participantId,
                        IntakeFormVersionId = formVersion.Id,
                        FieldResponses = fieldResponses,
                        HealthCampServiceAssignmentId = checkIn.HealthCampServiceAssignmentId // ✅ Now injected
                    };

                    // 🔍 DEBUG LOG
                    _logger.LogInformation("➡️ Submitting FormResponse for PatientNumber={PatientNumber}, DTO={@FormDto}",
                        patientNumber, formDto);

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
                    _logger.LogError(ex, "❌ Failed processing row {Row} on sheet {Sheet}", rowIndex, sheet.Name);
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

        // Collect intake form IDs directly from service/category/subcategory
        var intakeFormIds = new HashSet<Guid>();
        foreach (var assignment in scopedAssignments)
        {
            ct.ThrowIfCancellationRequested();

            switch (assignment.AssignmentType)
            {
                case PackageItemType.Service:
                    var service = await _serviceRepo.GetByIdAsync(assignment.AssignmentId);
                    if (service?.IntakeFormId != null)
                        intakeFormIds.Add(service.IntakeFormId.Value);
                    break;

                case PackageItemType.ServiceCategory:
                    var category = await _categoryRepo.GetByIdAsync(assignment.AssignmentId);
                    if (category?.IntakeFormId != null)
                        intakeFormIds.Add(category.IntakeFormId.Value);
                    break;

                case PackageItemType.ServiceSubcategory:
                    var subcategory = await _subcategoryRepo.GetByIdAsync(assignment.AssignmentId);
                    if (subcategory?.IntakeFormId != null)
                        intakeFormIds.Add(subcategory.IntakeFormId.Value);
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

        // Generate worksheet per lab form
        foreach (var form in formsToInclude)
        {
            ct.ThrowIfCancellationRequested();

            var sheet = package.Workbook.Worksheets.Add(form.Name);
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

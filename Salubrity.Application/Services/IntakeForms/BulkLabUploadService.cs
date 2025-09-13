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

    public BulkLabUploadService(
        IFormFieldMappingRepository mappingRepo,
        IIntakeFormResponseService formResponseService,
        IPatientRepository patientRepo,
        IIntakeFormRepository formRepo,
        ISubcontractorRepository subcontractorRepo,
        IHealthCampServiceAssignmentRepository assignmentRepo,
        IPackageReferenceResolver packageResolver
    )
    {
        _mappingRepo = mappingRepo;
        _formResponseService = formResponseService;
        _patientRepo = patientRepo;
        _formRepo = formRepo;
        _subcontractorRepo = subcontractorRepo;
        _assignmentRepo = assignmentRepo;
        _packageResolver = packageResolver;
    }

    public async Task<BulkUploadResultDto> UploadExcelAsync(CreateBulkLabUploadDto dto, CancellationToken ct = default)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Salubrity");

        var result = new BulkUploadResultDto();

        using var package = new ExcelPackage(dto.ExcelFile.OpenReadStream());
        foreach (var sheet in package.Workbook.Worksheets)
        {
            var formVersion = await _mappingRepo.GetFormVersionBySheetNameAsync(sheet.Name, ct);
            if (formVersion == null)
            {
                result.Errors.Add(new BulkUploadError
                {
                    Row = 0,
                    Message = $"No Intake Form configured for sheet: {sheet.Name}"
                });
                continue;
            }

            var mappings = await _mappingRepo.GetFieldMappingsAsync(formVersion.Id, ct);
            if (mappings.Count == 0) continue;

            for (int rowIndex = 2; rowIndex <= sheet.Dimension.End.Row; rowIndex++)
            {
                try
                {
                    var patientNumber = sheet.Cells[rowIndex, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(patientNumber))
                        throw new NotFoundException("Missing patient number");

                    var patientId = await _patientRepo.GetPatientIdByPatientNumberAsync(patientNumber, ct)
                                    ?? throw new NotFoundException($"Patient not found: {patientNumber}");

                    var fieldResponses = new List<CreateIntakeFormFieldResponseDto>();
                    int colIndex = 3;
                    foreach (var key in mappings.Keys)
                    {
                        var value = sheet.Cells[rowIndex, colIndex].Text?.Trim();
                        if (!string.IsNullOrEmpty(value))
                        {
                            fieldResponses.Add(new CreateIntakeFormFieldResponseDto
                            {
                                FieldId = mappings[key],
                                Value = value
                            });
                        }
                        colIndex++;
                    }

                    var formDto = new CreateIntakeFormResponseDto
                    {
                        PatientId = patientId,
                        IntakeFormVersionId = formVersion.Id,
                        FieldResponses = fieldResponses
                    };

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

        var subcontractor = await _subcontractorRepo.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("You are not linked to any provider profile.");

        var assignments = await _assignmentRepo.GetBySubcontractorIdAsync(subcontractor.Id, ct);

        var scopedAssignments = assignments
            .Where(a => a.HealthCampId == campId && a.HealthCamp != null && !a.HealthCamp.IsDeleted)
            .ToList();

        if (scopedAssignments.Count == 0)
            throw new NotFoundException("You are not assigned to the selected health camp.");

        var intakeFormIds = new HashSet<Guid>();

        foreach (var assignment in scopedAssignments)
        {
            ct.ThrowIfCancellationRequested();

            var service = await _packageResolver.ResolveServiceAsync(assignment.AssignmentId, assignment.AssignmentType);
            if (service?.IntakeFormId != null)
                intakeFormIds.Add(service.IntakeFormId.Value);
        }

        if (intakeFormIds.Count == 0)
            throw new NotFoundException("No intake forms assigned to your services in this camp.");

        var labForms = await _formRepo.GetAllLabFormsAsync(ct);
        var formsToInclude = labForms
            .Where(f => intakeFormIds.Contains(f.Id))
            .ToList();

        if (formsToInclude.Count == 0)
            throw new NotFoundException("No lab forms found for your assigned services.");

        var patients = await _patientRepo.GetPatientsByCampAsync(campId, ct);

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

            for (int col = 0; col < headers.Count; col++)
            {
                sheet.Cells[1, col + 1].Value = headers[col];
            }

            sheet.View.FreezePanes(2, 1);

            int rowIndex = 2;
            foreach (var patient in patients)
            {
                ct.ThrowIfCancellationRequested();

                if (patient.User == null)
                    continue;

                var fullName = string.Join(" ", new[]
                {
                patient.User.FirstName,
                patient.User.MiddleName,
                patient.User.LastName
            }.Where(n => !string.IsNullOrWhiteSpace(n)));

                sheet.Cells[rowIndex, 1].Value = patient.PatientNumber;
                sheet.Cells[rowIndex, 2].Value = fullName;

                for (int col = 3; col <= headers.Count; col++)
                {
                    sheet.Cells[rowIndex, col].Value = string.Empty;
                }

                rowIndex++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        package.Save();
        stream.Position = 0;
        return stream;
    }


}

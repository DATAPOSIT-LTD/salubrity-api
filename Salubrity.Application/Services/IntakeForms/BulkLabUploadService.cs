using System.Data;
using System.Text;
using OfficeOpenXml;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.Patients;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.IntakeForms;

public class BulkLabUploadService : IBulkLabUploadService
{
    private readonly IFormFieldMappingRepository _mappingRepo;
    private readonly IIntakeFormResponseService _formResponseService;
    private readonly IPatientRepository _patientRepo;
    private readonly IIntakeFormRepository _formRepo;

    public BulkLabUploadService(
        IFormFieldMappingRepository mappingRepo,
        IIntakeFormResponseService formResponseService,
        IPatientRepository patientRepo,
        IIntakeFormRepository formRepo
    )
    {
        _mappingRepo = mappingRepo;
        _formResponseService = formResponseService;
        _patientRepo = patientRepo;
        _formRepo = formRepo;
    }

    /// <summary>
    /// Upload lab results from Excel file.
    /// Maps patient rows and fields to the correct form version.
    /// </summary>
    public async Task<BulkUploadResultDto> UploadExcelAsync(CreateBulkLabUploadDto dto, CancellationToken ct = default)
    {
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
            if (!mappings.Any()) continue;

            // Read patient rows
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
                    int colIndex = 3; // first 2 columns are Patient Number & Full Name
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

    /// <summary>
    /// Generate a single Excel workbook containing all lab forms with patient info.
    /// </summary>
    public async Task<Stream> GenerateAllLabTemplatesExcelAsync(CancellationToken ct = default)
    {
        var stream = new MemoryStream();
        using var package = new ExcelPackage(stream);

        var labForms = await _formRepo.GetAllLabFormsAsync(ct);
        var patients = await _patientRepo.GetAllPatientsAsync(ct);

        foreach (var form in labForms)
        {
            var sheet = package.Workbook.Worksheets.Add(form.Name);

            // Headers: Patient Number, Full Name + Lab fields
            var headers = new List<string> { "Patient Number", "Patient Full Name" };
            foreach (var section in form.Sections.OrderBy(s => s.Order))
                headers.AddRange(section.Fields.OrderBy(f => f.Order).Select(f => f.Label));

            for (int col = 0; col < headers.Count; col++)
                sheet.Cells[1, col + 1].Value = headers[col];

            sheet.View.FreezePanes(2, 1);

            int rowIndex = 2;
            foreach (var patient in patients)
            {
                var fullName = string.Join(' ',
                    new[] { patient.User.FirstName, patient.User.MiddleName, patient.User.LastName }
                    .Where(n => !string.IsNullOrEmpty(n)));

                sheet.Cells[rowIndex, 1].Value = patient.PatientNumber;
                sheet.Cells[rowIndex, 2].Value = fullName;

                for (int col = 3; col <= headers.Count; col++)
                    sheet.Cells[rowIndex, col].Value = "";

                rowIndex++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        package.Save();
        stream.Position = 0;
        return stream;
    }
}

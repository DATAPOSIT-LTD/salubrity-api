using System.Data;
using System.Text;
using ExcelDataReader;
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

    public BulkLabUploadService(
        IFormFieldMappingRepository mappingRepo,
        IIntakeFormResponseService formResponseService,
        IPatientRepository patientRepo)
    {
        _mappingRepo = mappingRepo;
        _formResponseService = formResponseService;
        _patientRepo = patientRepo;
    }

    public async Task<BulkUploadResultDto> UploadCsvAsync(CreateBulkLabUploadDto dto, CancellationToken ct = default)
    {
        var result = new BulkUploadResultDto();

        using var stream = dto.CsvFile.OpenReadStream();
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = true
            }
        });

        foreach (DataTable sheet in dataSet.Tables)
        {
            var formVersion = await _mappingRepo.GetFormVersionBySheetNameAsync(sheet.TableName, ct);
            if (formVersion == null)
            {
                result.Errors.Add(new BulkUploadError
                {
                    Row = 0,
                    Message = $"No Intake Form configured for sheet: {sheet.TableName}"
                });
                continue;
            }

            var mappings = await _mappingRepo.GetFieldMappingsAsync(formVersion.Id, ct);
            if (!mappings.Any()) continue;

            for (int rowIndex = 0; rowIndex < sheet.Rows.Count; rowIndex++)
            {
                try
                {
                    var row = sheet.Rows[rowIndex];
                    var dict = sheet.Columns.Cast<DataColumn>()
                                   .ToDictionary(c => c.ColumnName.Trim().ToLower(), c => row[c]?.ToString()?.Trim());

                    var patientNumber = dict.GetValueOrDefault("patient number");
                    if (string.IsNullOrEmpty(patientNumber))
                        throw new NotFoundException("Missing patient number");

                    var patientId = await _patientRepo.GetPatientIdByPatientNumberAsync(patientNumber, ct)
                        ?? throw new NotFoundException($"Patient not found: {patientNumber}");

                    var fieldResponses = new List<CreateIntakeFormFieldResponseDto>();
                    foreach (var kvp in dict)
                    {
                        if (mappings.TryGetValue(kvp.Key, out var fieldId) && !string.IsNullOrEmpty(kvp.Value))
                        {
                            fieldResponses.Add(new CreateIntakeFormFieldResponseDto
                            {
                                FieldId = fieldId,
                                Value = kvp.Value
                            });
                        }
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
                        Row = rowIndex + 2, // +2 because ExcelDataReader rows are zero-indexed and header row skipped
                        Message = ex.Message
                    });
                }
            }
        }

        return result;
    }

    public async Task<Stream> GenerateCsvTemplateAsync(string sheetName, CancellationToken ct = default)
    {
        var formVersion = await _mappingRepo.GetFormVersionBySheetNameAsync(sheetName, ct);
        if (formVersion == null)
            throw new NotFoundException($"No Intake Form configured for sheet: {sheetName}");

        var mappings = await _mappingRepo.GetFieldMappingsAsync(formVersion.Id, ct);
        if (!mappings.Any())
            throw new NotFoundException("No field mappings configured");

        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        // Write header
        await writer.WriteLineAsync(string.Join(',', mappings.Keys));

        await writer.FlushAsync();
        stream.Position = 0;
        return stream;
    }
}

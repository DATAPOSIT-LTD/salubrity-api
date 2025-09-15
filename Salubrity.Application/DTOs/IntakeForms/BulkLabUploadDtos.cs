using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Salubrity.Application.DTOs.IntakeForms;

public class BulkUploadResultDto
{
    public int SuccessCount { get; set; } = 0;
    public int FailureCount { get; set; } = 0;
    public List<BulkUploadError> Errors { get; set; } = new();
}

public class BulkUploadError
{
    public int Row { get; set; }
    public string Message { get; set; } = null!;
}

public class CreateBulkLabUploadDto
{
    public IFormFile ExcelFile { get; set; } = null!;
    public Guid IntakeFormVersionId { get; set; }
    public Guid SubmittedByUserId { get; set; }
}

public class UploadLabResultsFormDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}

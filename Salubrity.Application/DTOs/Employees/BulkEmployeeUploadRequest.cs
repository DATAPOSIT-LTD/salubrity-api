using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class BulkEmployeeUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = default!;
}

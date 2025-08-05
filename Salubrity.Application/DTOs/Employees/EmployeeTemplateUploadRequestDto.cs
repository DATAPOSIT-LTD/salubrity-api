using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class EmployeeTemplateUploadRequestDto
{
    [Required]
    public required IFormFile File { get; set; }
}

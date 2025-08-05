using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Salubrity.Application.Interfaces.Services.Employee;

public class EmployeeTemplateService : IEmployeeTemplateService
{
    private readonly string _templateDirectory;
    private readonly string _templateFileName = "EmployeeTemplate.xlsx";

    public EmployeeTemplateService()
    {
        _templateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Employees");
    }

    public async Task<string> ProcessAndUpsertTemplateAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("No file uploaded.");

        if (!Directory.Exists(_templateDirectory))
            Directory.CreateDirectory(_templateDirectory);

        var filePath = Path.Combine(_templateDirectory, _templateFileName);

        //  Delete the existing template if it exists
        if (File.Exists(filePath))
            File.Delete(filePath);

        //  Save the new template
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return "Template uploaded and replaced successfully.";
    }

    public async Task<(byte[] Content, string FileName)> GetTemplateAsync()
    {
        var path = Path.Combine(_templateDirectory, _templateFileName);

        if (!File.Exists(path))
            throw new FileNotFoundException("Employee template not found.");

        var bytes = await File.ReadAllBytesAsync(path);
        return (bytes, _templateFileName);
    }
}

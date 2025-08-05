using Microsoft.AspNetCore.Http;

public interface IEmployeeTemplateService
{
    Task<string> ProcessAndUpsertTemplateAsync(IFormFile file);
    Task<(byte[] Content, string FileName)> GetTemplateAsync();
}

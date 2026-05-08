// File: Application/DTOs/Reports/SendCorporateReportEmailRequest.cs
namespace Salubrity.Application.DTOs.Reports;

public class SendCorporateReportEmailRequest
{
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

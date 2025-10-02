// File: Salubrity.Application/Interfaces/Services/Forms/IIntakeFormResponseService.cs

using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.IntakeForms;

namespace Salubrity.Application.Interfaces.Services.IntakeForms;

public interface IIntakeFormResponseService
{
    Task<Guid> SubmitResponseAsync(CreateIntakeFormResponseDto dto, Guid userId, CancellationToken ct = default);
    Task<List<IntakeFormResponseDetailDto>> GetResponsesByPatientAndCampIdAsync(Guid patientId, Guid healthCampId, CancellationToken ct = default);

    // Download Findings Implementation
    Task<(byte[] ExcelData, string CampName, string OrganizationName)> ExportCampDataToExcelAsync(Guid campId, CancellationToken ct = default);
    Task<(byte[] ExcelData, string CampName, string OrganizationName)> ExportCampDataToExcelSheetAsync(Guid campId, CancellationToken ct = default);
    Task<(byte[] ExcelData, string CampName, string OrganizationName)> ExportCampDataToExcelSheetStyledAsync(Guid campId, CancellationToken ct = default);
}

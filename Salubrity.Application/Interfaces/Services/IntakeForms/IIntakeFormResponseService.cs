// File: Salubrity.Application/Interfaces/Services/Forms/IIntakeFormResponseService.cs

using Salubrity.Application.DTOs.IntakeForms;

namespace Salubrity.Application.Interfaces.Services.IntakeForms;

public interface IIntakeFormResponseService
{
    Task<Guid> SubmitResponseAsync(CreateIntakeFormResponseDto dto, Guid userId, CancellationToken ct = default);
    Task<List<IntakeFormResponseDto>> GetResponsesByPatientAndCampIdAsync(Guid patientId, Guid healthCampId, CancellationToken ct = default);
}

using Salubrity.Application.DTOs.Clinical;

namespace Salubrity.Application.Interfaces.Services.Clinical;

public interface IClinicalFieldDraftService
{
    Task<string> GenerateAsync(GenerateClinicalFieldDraftRequestDto request, CancellationToken ct = default);
}

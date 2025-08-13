// File: Salubrity.Application/Interfaces/Services/Forms/IFormBuilderService.cs
#nullable enable
using Salubrity.Application.DTOs.Forms;

namespace Salubrity.Application.Interfaces.Services.IntakeForms
{
    public interface IFormBuilderService
    {
        Task<FormBlueprintResponseDto> CreateFromBlueprintAsync(CreateFormBlueprintDto dto, CancellationToken ct = default);
    }
}

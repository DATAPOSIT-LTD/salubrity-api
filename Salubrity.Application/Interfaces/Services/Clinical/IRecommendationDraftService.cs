// File: Application/Interfaces/Services/Clinical/IRecommendationDraftService.cs
using Salubrity.Application.DTOs.Clinical;

namespace Salubrity.Application.Interfaces.Services.Clinical;

public interface IRecommendationDraftService
{
    /// <summary>
    /// Generates an AI-assisted draft recommendation for the doctor, grounded in
    /// (a) the patient's abnormal + borderline findings for this camp, and
    /// (b) whatever the doctor has already typed into their review form.
    /// The doctor is expected to review and edit before saving.
    /// </summary>
    Task<string> GenerateAsync(GenerateDraftRequestDto request, CancellationToken ct = default);
}

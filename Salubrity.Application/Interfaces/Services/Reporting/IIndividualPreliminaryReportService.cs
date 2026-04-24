using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Interfaces.Services.Reporting;

public interface IIndividualPreliminaryReportService
{
    /// <summary>
    /// Build the full Individual Preliminary Report payload for a single participant.
    /// Pulls demographics, classifies every vital reading, and aggregates the score + risk bars.
    /// </summary>
    Task<IndividualPreliminaryReportDto> BuildAsync(Guid participantId, CancellationToken ct = default);

    /// <summary>
    /// Returns the rendered PDF bytes for the Individual Preliminary Report.
    /// </summary>
    Task<byte[]> BuildPdfAsync(Guid participantId, CancellationToken ct = default);
}

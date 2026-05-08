using Salubrity.Application.DTOs.Bi;

namespace Salubrity.Application.Interfaces.Repositories.Bi;

public interface IBiRepository
{
    Task<List<BiOrganizationDto>> GetOrganizationsAsync(CancellationToken ct = default);

    Task<List<BiCampDto>> GetCampsAsync(Guid? orgId, int? year, CancellationToken ct = default);

    Task<List<BiPatientRowDto>> GetPatientRowsAsync(Guid? orgId, Guid? campId, int? year, CancellationToken ct = default);

    Task<List<BiFindingDto>> GetFindingsAsync(Guid? orgId, Guid? campId, int? year, CancellationToken ct = default);

    Task<List<BiLabResultDto>> GetLabResultsAsync(Guid? orgId, Guid? campId, int? year, CancellationToken ct = default);
}

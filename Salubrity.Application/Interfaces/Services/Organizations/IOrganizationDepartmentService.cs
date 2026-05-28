using Salubrity.Application.DTOs.Organizations;

namespace Salubrity.Application.Interfaces.Services.Organizations;

public interface IOrganizationDepartmentService
{
    Task<List<OrganizationDepartmentDto>> ListAsync(Guid organizationId, CancellationToken ct = default);
    Task<OrganizationDepartmentDto> AddAsync(Guid organizationId, CreateOrgDepartmentDto dto, CancellationToken ct = default);
    Task<BulkDepartmentUploadResultDto> UploadAsync(Guid organizationId, Stream xlsxStream, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<byte[]> GenerateTemplateAsync(CancellationToken ct = default);
}

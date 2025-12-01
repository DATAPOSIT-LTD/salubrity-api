using Microsoft.AspNetCore.Http;
using Salubrity.Application.DTOs.Organizations.Branches;

namespace Salubrity.Application.Interfaces.Services.Organizations;

public interface IOrganizationBranchService
{
    Task<OrganizationBranchResponseDto> CreateAsync(Guid organizationId, OrganizationBranchCreateDto dto);
    Task<List<OrganizationBranchResponseDto>> GetForOrganizationAsync(Guid organizationId);
    Task<OrganizationBranchResponseDto> GetByIdAsync(Guid id);
    Task UpdateAsync(Guid id, OrganizationBranchUpdateDto dto);
    Task DeleteAsync(Guid id);

    Task BulkUploadAsync(Guid organizationId, IFormFile csvFile);
}

using Salubrity.Application.DTOs.Organizations;

namespace Salubrity.Application.Interfaces.Services.Organizations
{
    public interface IOrganizationService
    {
        Task<OrganizationResponseDto> CreateAsync(OrganizationCreateDto dto);
        Task<OrganizationResponseDto> GetByIdAsync(Guid id);
        Task<List<OrganizationResponseDto>> GetAllAsync();
        Task UpdateAsync(Guid id, OrganizationUpdateDto dto);
        Task DeleteAsync(Guid id);
    }
}

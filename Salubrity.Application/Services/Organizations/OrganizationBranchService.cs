using AutoMapper;
using Microsoft.AspNetCore.Http;
using Salubrity.Application.DTOs.Organizations.Branches;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Organizations;

public class OrganizationBranchService : IOrganizationBranchService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IOrganizationBranchRepository _branchRepository;
    private readonly IMapper _mapper;

    public OrganizationBranchService(
        IOrganizationRepository organizationRepository,
        IOrganizationBranchRepository branchRepository,
        IMapper mapper)
    {
        _organizationRepository = organizationRepository;
        _branchRepository = branchRepository;
        _mapper = mapper;
    }

    public async Task<OrganizationBranchResponseDto> CreateAsync(Guid organizationId, OrganizationBranchCreateDto dto)
    {
        var org = await _organizationRepository.GetByIdAsync(organizationId);
        if (org == null)
            throw new NotFoundException("Organization", organizationId.ToString());

        var entity = _mapper.Map<OrganizationBranch>(dto);
        entity.OrganizationId = organizationId;

        var created = await _branchRepository.CreateAsync(entity);
        return _mapper.Map<OrganizationBranchResponseDto>(created);
    }

    public async Task<List<OrganizationBranchResponseDto>> GetForOrganizationAsync(Guid organizationId)
    {
        var branches = await _branchRepository.GetByOrganizationAsync(organizationId);
        return _mapper.Map<List<OrganizationBranchResponseDto>>(branches);
    }

    public async Task<OrganizationBranchResponseDto> GetByIdAsync(Guid id)
    {
        var branch = await _branchRepository.GetByIdAsync(id);
        if (branch == null)
            throw new NotFoundException("OrganizationBranch", id.ToString());

        return _mapper.Map<OrganizationBranchResponseDto>(branch);
    }

    public async Task UpdateAsync(Guid id, OrganizationBranchUpdateDto dto)
    {
        var branch = await _branchRepository.GetByIdAsync(id);
        if (branch == null)
            throw new NotFoundException("OrganizationBranch", id.ToString());

        _mapper.Map(dto, branch);

        await _branchRepository.UpdateAsync(branch);
    }

    public async Task DeleteAsync(Guid id)
    {
        var branch = await _branchRepository.GetByIdAsync(id);
        if (branch == null)
            throw new NotFoundException("OrganizationBranch", id.ToString());

        await _branchRepository.DeleteAsync(id);
    }

    public async Task BulkUploadAsync(Guid organizationId, IFormFile csvFile)
    {
        // implement after we define the CSV template
        throw new NotImplementedException("Bulk upload not implemented yet.");
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Salubrity.Application.DTOs.Subcontractor;

namespace Salubrity.Application.Interfaces.Services
{
    public interface ISubcontractorService
    {
        Task<SubcontractorDto> CreateAsync(CreateSubcontractorDto dto);
        Task<SubcontractorDto> UpdateAsync(Guid id, UpdateSubcontractorDto dto);
        Task<SubcontractorDto> GetByIdAsync(Guid id);
        Task<List<SubcontractorDto>> GetAllAsync();
        Task AssignRoleAsync(Guid subcontractorId, AssignSubcontractorRoleDto dto);
        Task AssignSpecialtyAsync(Guid subcontractorId, CreateSubcontractorSpecialtyDto dto);
    }
}

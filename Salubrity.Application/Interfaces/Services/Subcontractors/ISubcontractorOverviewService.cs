using Salubrity.Application.DTOs.Subcontractor;

namespace Salubrity.Application.Interfaces.Services.Subcontractors
{
    public interface ISubcontractorOverviewService
    {
        Task<SubcontractorOverviewDto> GetSubcontractorOverviewAsync();
    }
}

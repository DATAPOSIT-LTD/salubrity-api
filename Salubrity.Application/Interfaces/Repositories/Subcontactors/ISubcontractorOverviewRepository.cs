using Salubrity.Application.DTOs.Subcontractor;

namespace Salubrity.Application.Interfaces.Repositories.Subcontactors
{
    public interface ISubcontractorOverviewRepository
    {
        Task<SubcontractorOverviewDto> GetSubcontractorOverviewAsync();
    }
}

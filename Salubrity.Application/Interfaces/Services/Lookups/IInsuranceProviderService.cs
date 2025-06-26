using Salubrity.Application.DTOs.Lookups;
using Salubrity.Shared.Responses;

namespace Salubrity.Application.Interfaces.Services.Lookups;

public interface IInsuranceProviderService
{
	Task<ApiResponse<List<InsuranceProviderResponse>>> GetAllAsync();
}

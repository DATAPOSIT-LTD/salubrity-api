using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Services.Lookups;
using Salubrity.Application.Services.Lookups;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Lookups;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups/insurance-providers")]
[Produces("application/json")]
[Tags("Lookup: Insurance Providers")]
public class InsuranceProviderController : BaseController
{
    private readonly IInsuranceProviderService _service;

    public InsuranceProviderController(IInsuranceProviderService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all insurance providers with logo URLs.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<InsuranceProviderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
}

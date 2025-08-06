// File: Api/Controllers/LookupsController.cs

using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Services.Lookups;
using Salubrity.Application.Lookups;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers;

/// <summary>
/// Provides access to generic lookup data (e.g., languages, genders, departments).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups")]
[Produces("application/json")]
[Tags("Generic Lookups")]
public class LookupsController : BaseController
{
    private readonly ILookupServiceFactory _factory;

    public LookupsController(ILookupServiceFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Gets all items for a given lookup.
    /// </summary>
    [HttpGet("{lookupName}")]
    [ProducesResponseType(typeof(ApiResponse<List<BaseLookupResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(string lookupName)
    {
        var service = _factory.Resolve(lookupName);
        var result = await service.GetAllAsync();
        return Success(result);
    }

    /// <summary>
    /// Lists all supported lookup types.
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public IActionResult GetAvailableLookups()
    {
        var lookups = LookupRegistry.LookupMap.Keys.OrderBy(x => x).ToList();
        return Success(lookups);
    }
}

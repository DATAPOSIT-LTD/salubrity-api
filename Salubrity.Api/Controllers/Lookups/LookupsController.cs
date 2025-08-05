// File: Api/Controllers/LookupsController.cs

using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Services.Lookups;
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
    /// <param name="lookupName">The name of the lookup (e.g., "languages", "genders").</param>
    /// <returns>A list of lookup items.</returns>
    /// <remarks>
    ///  Example usage:
    ///
    ///     GET /api/v1/lookups/languages
    ///
    ///     Response:
    ///     {
    ///         "success": true,
    ///         "data": [
    ///             { "id": "guid-1", "name": "English", "description": "English Language" },
    ///             { "id": "guid-2", "name": "French", "description": "French Language" }
    ///         ]
    ///     }
    /// </remarks>
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
    /// <returns>A list of available lookup names.</returns>
    /// <remarks>
    ///  Use this to discover valid lookup types for use in the main endpoint.
    ///
    ///     GET /api/v1/lookups/available
    ///
    ///     Response:
    ///     {
    ///         "success": true,
    ///         "data": ["languages", "genders", "departments", "jobtitles"]
    ///     }
    /// </remarks>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public IActionResult GetAvailableLookups()
    {
        var lookups = new List<string>
        {
            "languages",
            "genders",
            "departments",
            "jobtitles"

        };

        return Success(lookups);
    }
}

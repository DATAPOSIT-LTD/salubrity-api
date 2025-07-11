// File: Api/Controllers/Lookups/GenderController.cs

using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Shared.Responses;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups/genders")]
[Produces("application/json")]
[Tags("Lookup: Genders")]
public class GenderController : BaseController
{
    private readonly ILookupService _genderService;

    public GenderController(ILookupService genderService)
    {
        _genderService = genderService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BaseLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await _genderService.GetAllAsync();
        return Success(result);

    }
}

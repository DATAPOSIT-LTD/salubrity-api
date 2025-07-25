// File: Api/Controllers/Lookups/GenderController.cs

using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Lookups;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups/genders")]
[Produces("application/json")]
[Tags("Lookup: Genders")]
public class GenderController : BaseController
{
    private readonly ILookupRepository<Gender> _repository;

    public GenderController(ILookupRepository<Gender> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BaseLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var service = new GenericLookupService<Gender>(_repository);
        var result = await service.GetAllAsync();
        return Success(result);
    }
}

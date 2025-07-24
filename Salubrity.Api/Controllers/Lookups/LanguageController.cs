// File: Api/Controllers/Lookups/LanguageController.cs

using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Lookups;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups/languages")]
[Produces("application/json")]
[Tags("Lookup: Languages")]
public class LanguageController : BaseController
{
    private readonly ILookupRepository<Language> _repository;

    public LanguageController(ILookupRepository<Language> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BaseLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var service = new GenericLookupService<Language>(_repository);
        var result = await service.GetAllAsync();
        return Success(result);
    }
}

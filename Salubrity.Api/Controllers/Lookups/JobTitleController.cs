// File: Api/Controllers/Lookups/JobTitleController.cs

using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Lookups;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups/job-titles")]
[Produces("application/json")]
[Tags("Lookup: Job Titles")]
public class JobTitleController : BaseController
{
    private readonly ILookupRepository<JobTitle> _repository;

    public JobTitleController(ILookupRepository<JobTitle> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BaseLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var service = new GenericLookupService<JobTitle>(_repository);
        var result = await service.GetAllAsync();
        return Success(result);
    }
}

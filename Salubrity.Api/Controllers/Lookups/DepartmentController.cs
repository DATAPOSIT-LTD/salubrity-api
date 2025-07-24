// File: Api/Controllers/Lookups/DepartmentController.cs

using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Lookups;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups/departments")]
[Produces("application/json")]
[Tags("Lookup: Departments")]
public class DepartmentController : BaseController
{
    private readonly ILookupRepository<Department> _repository;

    public DepartmentController(ILookupRepository<Department> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BaseLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var service = new GenericLookupService<Department>(_repository);
        var result = await service.GetAllAsync();
        return Success(result);
    }
}

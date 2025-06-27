using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.HealthcareServices;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/service-packages")]
[Produces("application/json")]
[Tags("Service Packages")]
public class ServicePackageController : BaseController
{
    private readonly IServicePackageService _service;

    public ServicePackageController(IServicePackageService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ServicePackageResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Success(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ServicePackageResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
        => Success(await _service.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ServicePackageResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateServicePackageDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ServicePackageResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServicePackageDto dto)
        => Success(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Success("Service package deleted.");
    }
}

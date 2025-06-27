using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.HealthcareServices;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/services")]
[Produces("application/json")]
[Tags("Services Management")]
public class ServiceController : BaseController
{
    private readonly IServiceService _service;

    public ServiceController(IServiceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ServiceResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Success(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ServiceResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Success(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Success("Service deleted.");
    }
}

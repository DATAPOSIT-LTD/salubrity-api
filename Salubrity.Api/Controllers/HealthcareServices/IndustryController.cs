using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Shared.Responses;
using Salubrity.Api.Controllers.Common;

namespace Salubrity.Api.Controllers.Services;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/industries")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("Industry Management")]
public class IndustryController : BaseController
{
    private readonly IIndustryService _service;

    public IndustryController(IIndustryService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<IndustryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Success(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IndustryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IndustryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateIndustryDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IndustryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIndustryDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Success(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Success("Deleted successfully.");
    }
}

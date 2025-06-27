using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Camps;
using Salubrity.Application.Interfaces.Services.Camps;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Camps;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/camps")]
[Produces("application/json")]
[Tags("Camps Management")]
public class CampController : BaseController
{
    private readonly ICampService _service;

    public CampController(ICampService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CampDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Success(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CampDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CampDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCampDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CampDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Success(result);
    }

    //[HttpDelete("{id:guid}")]
    //[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    //public async Task<IActionResult> Delete(Guid id)
    //{
    //    await _service.DeleteAsync(id);
    //    return Success("Camp deleted.");
    //}
}

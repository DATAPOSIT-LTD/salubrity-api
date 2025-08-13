// File: Salubrity.Api/Controllers/Forms/FormBuilderController.cs
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Forms;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/forms/blueprint")]
[Produces("application/json")]
[Tags("Intake Forms Management")]
public class FormBuilderController : BaseController
{
    private readonly IFormBuilderService _service;

    public FormBuilderController(IFormBuilderService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FormBlueprintResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFormBlueprintDto dto)
    {
        var result = await _service.CreateFromBlueprintAsync(dto);
        // Point Location header to this controller's GetById
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FormBlueprintResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Minimal echo to satisfy CreatedSuccess location pattern.
        // If you want full FormResponseDto, wire IFormService here and return that instead.
        var result = new FormBlueprintResponseDto { Id = id, Name = "Created" };
        return Success(result);
    }
}

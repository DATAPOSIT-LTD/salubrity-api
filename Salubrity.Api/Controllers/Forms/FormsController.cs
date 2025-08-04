using Microsoft.AspNetCore.Mvc;
using Salubrity.Application.Interfaces.Services.Forms;
using Salubrity.Shared.Responses;
using Salubrity.Api.Controllers.Common;

namespace Salubrity.Api.Controllers.Forms;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/forms")]
[Produces("application/json")]
[Tags("Intake Forms Management")]

public class FormsController : BaseController
{
    private readonly IFormService _formService;

    public FormsController(IFormService formService)
    {
        _formService = formService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FormResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FormResponseDto>>>> GetAllForms()
    {
        var response = await _formService.GetAllFormsAsync();
        return response;


    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<FormResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FormResponseDto>>> GetFormById(Guid id)
    {
        var response = await _formService.GetFormByIdAsync(id);
        return response;
    }
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FormResponseDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<FormResponseDto>>> CreateForm([FromBody] CreateFormRequestDto request)
    {
        var response = await _formService.CreateFormAsync(request);
        if (response.Data == null)
        {
            return BadRequest(response);
        }
        return CreatedAtAction(nameof(GetFormById), new { id = response.Data.Id }, response);
    }
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<FormResponseDto>), StatusCodes.Status201Created)]
    public async Task<ApiResponse<bool>> DeleteFormAsync(Guid formId)
    {

        var response = await _formService.DeleteFormAsync(formId);
        return response;
    }
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FormResponseDto>), StatusCodes.Status201Created)]
    public async Task<ApiResponse<FormFieldResponseDto>> AddFieldToFormAsync(Guid formId, CreateFormFieldRequestDto request)
    {
        var response = await _formService.AddFieldToFormAsync(formId, request);
        return response;
     }
}
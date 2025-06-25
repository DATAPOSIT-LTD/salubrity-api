using Microsoft.AspNetCore.Mvc;
using Salubrity.Application.DTOs.Organizations;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Shared.Responses;
using Salubrity.Api.Controllers.Common;

namespace Salubrity.Api.Controllers.Organizations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/organizations")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("Organizations Management")]
public class OrganizationController : BaseController
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] OrganizationCreateDto input)
    {
        var result = await _organizationService.CreateAsync(input);
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}", Name = "GetOrganizationById")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _organizationService.GetByIdAsync(id);
        return Success(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<OrganizationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _organizationService.GetAllAsync();
        return Success(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] OrganizationUpdateDto input)
    {
        await _organizationService.UpdateAsync(id, input);
        return SuccessMessage("Updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _organizationService.DeleteAsync(id);
        return SuccessMessage("Deleted successfully.");
    }
}

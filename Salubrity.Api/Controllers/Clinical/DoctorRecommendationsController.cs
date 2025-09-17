// File: Api/Controllers/Clinical/DoctorRecommendationsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Clinical;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = "Doctor,Admin")]
[Route("api/v{version:apiVersion}/doctor-recommendations")]
[Produces("application/json")]
[Tags("Doctor Recommendations")]
public class DoctorRecommendationsController : BaseController
{
    private readonly IDoctorRecommendationService _service;

    public DoctorRecommendationsController(IDoctorRecommendationService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DoctorRecommendationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Success(result);
    }

    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DoctorRecommendationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatient(Guid patientId, CancellationToken ct = default)
    {
        var result = await _service.GetByPatientAsync(patientId, ct);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRecommendationDto dto, CancellationToken ct = default)
    {
        var doctorId = GetCurrentUserId();
        var id = await _service.CreateAsync(dto, doctorId, ct);
        return Success(id);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRecommendationDto dto, CancellationToken ct = default)
    {
        dto.Id = id;
        await _service.UpdateAsync(dto, ct);
        return Success("Updated successfully");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
        return Success("Deleted successfully");
    }
}

using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Authorization;
using Salubrity.Application.DTOs.Bi;
using Salubrity.Application.Interfaces.Repositories.Bi;

namespace Salubrity.Api.Controllers.Bi;

/// <summary>
/// Read-only endpoints consumed by Power BI's Web data source. Authenticated by
/// X-BI-Key header (NOT JWT) so PBI scheduled refresh can authenticate with
/// stored credentials.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bi")]
[Tags("BI")]
[BiApiKey]
public class BiController : ControllerBase
{
    private readonly IBiRepository _repo;
    public BiController(IBiRepository repo) => _repo = repo;

    /// <summary>Dimension: organizations.</summary>
    [HttpGet("organizations")]
    public async Task<IActionResult> Organizations(CancellationToken ct)
        => Ok(await _repo.GetOrganizationsAsync(ct));

    /// <summary>Dimension: camps. Filter by orgId and/or year.</summary>
    [HttpGet("camps")]
    public async Task<IActionResult> Camps([FromQuery] Guid? orgId, [FromQuery] int? year, CancellationToken ct = default)
        => Ok(await _repo.GetCampsAsync(orgId, year, ct));

    /// <summary>Fact: one wide row per (patient, camp).</summary>
    [HttpGet("patients")]
    public async Task<IActionResult> Patients([FromQuery] Guid? orgId, [FromQuery] Guid? campId, [FromQuery] int? year, CancellationToken ct = default)
        => Ok(await _repo.GetPatientRowsAsync(orgId, campId, year, ct));

    /// <summary>Fact: one row per finding.</summary>
    [HttpGet("findings")]
    public async Task<IActionResult> Findings([FromQuery] Guid? orgId, [FromQuery] Guid? campId, [FromQuery] int? year, CancellationToken ct = default)
        => Ok(await _repo.GetFindingsAsync(orgId, campId, year, ct));

    /// <summary>Fact: one row per lab test result.</summary>
    [HttpGet("lab-results")]
    public async Task<IActionResult> LabResults([FromQuery] Guid? orgId, [FromQuery] Guid? campId, [FromQuery] int? year, CancellationToken ct = default)
        => Ok(await _repo.GetLabResultsAsync(orgId, campId, year, ct));
}

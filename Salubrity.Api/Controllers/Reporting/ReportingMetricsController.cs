using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Reporting;
using Salubrity.Application.Interfaces.Services.Reporting;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Reporting;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/reporting/metrics")]
[Produces("application/json")]
[Tags("Reporting Metrics")]
public class ReportingMetricsController : BaseController
{
    private readonly IReportingMetricService _service;

    public ReportingMetricsController(IReportingMetricService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ReportingMetricDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct = default)
    {
        var result = await _service.ListAsync(ct);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReportingMetricDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateReportingMetricDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Success(result);
    }

    [HttpGet("{metricCode}")]
    [ProducesResponseType(typeof(ApiResponse<ReportingMetricDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCode([FromRoute] string metricCode, CancellationToken ct = default)
    {
        var result = await _service.GetByCodeAsync(metricCode, ct);
        return Success(result);
    }
}

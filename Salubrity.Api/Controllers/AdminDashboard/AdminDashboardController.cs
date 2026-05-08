// File: Api/Controllers/AdminDashboard/AdminDashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.AdminDashboard;
using Salubrity.Application.Interfaces.Services.AdminDashboard;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.AdminDashboard;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/dashboard")]
[Tags("AdminDashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : BaseController
{
    private readonly IAdminDashboardOverviewService _svc;
    public AdminDashboardController(IAdminDashboardOverviewService svc) => _svc = svc;

    /// <summary>Top-of-page band for the admin dashboard: ongoing/upcoming/last camps, YTD totals, pending publishes.</summary>
    [HttpGet("overview")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardOverviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview([FromQuery] int? year, CancellationToken ct = default)
    {
        var dto = await _svc.GetOverviewAsync(year, ct);
        return Success(dto);
    }
}

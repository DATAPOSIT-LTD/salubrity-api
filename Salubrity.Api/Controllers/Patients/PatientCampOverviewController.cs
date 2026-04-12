using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Patients
{
    /// <summary>
    /// Camp overview for the current user (participant). Use the authenticated user's id from GET /api/v1/auth/me (e.g. id or sub claim).
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/patients")]
    [Produces("application/json")]
    [Tags("Patient Camp Overview")]
    public class PatientCampOverviewController : BaseController
    {
        private readonly IHealthCampOverviewService _service;

        public PatientCampOverviewController(IHealthCampOverviewService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get camp overview for a user (participant): camps attended (completed) and upcoming camps they are registered for.
        /// </summary>
        /// <param name="userId">User ID (e.g. from /auth/me — the authenticated user's id). Participations are matched by UserId.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("{userId:guid}/camp-overview")]
        [ProducesResponseType(typeof(ApiResponse<PatientCampOverviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPatientCampOverview(Guid userId, CancellationToken ct = default)
        {
            var result = await _service.GetPatientCampOverviewAsync(userId, ct);
            return Success(result);
        }
    }
}

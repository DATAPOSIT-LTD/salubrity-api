using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Patients
{
    /// <summary>
    /// Camp overview for a specific patient (participant). Use patientId from GET /api/v1/auth/me (relatedEntityId when relatedEntityType is "Patient").
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
        /// Get camp overview for a patient: camps attended (completed) and upcoming camps they are registered for.
        /// </summary>
        /// <param name="patientId">Patient ID (e.g. from /auth/me as relatedEntityId when relatedEntityType is "Patient").</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("{patientId:guid}/camp-overview")]
        [ProducesResponseType(typeof(ApiResponse<PatientCampOverviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPatientCampOverview(Guid patientId, CancellationToken ct = default)
        {
            var result = await _service.GetPatientCampOverviewAsync(patientId, ct);
            return Success(result);
        }
    }
}

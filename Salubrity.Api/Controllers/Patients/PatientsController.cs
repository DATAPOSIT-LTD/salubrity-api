using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Patients
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [Route("api/v{version:apiVersion}/patients")]
    [Produces("application/json")]
    [Tags("Patients")]
    public class PatientsController : BaseController
    {
        private readonly IPatientNumberGeneratorService _numberGenerator;
        private readonly IHealthCampOverviewService _campOverviewService;

        public PatientsController(
            IPatientNumberGeneratorService numberGenerator,
            IHealthCampOverviewService campOverviewService)
        {
            _numberGenerator = numberGenerator;
            _campOverviewService = campOverviewService;
        }

        /// <summary>
        /// Generate a new unique patient number
        /// </summary>
        [HttpPost("generate-number")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateNumber(CancellationToken ct = default)
        {
            var patientNumber = await _numberGenerator.GenerateAsync(ct);
            return Success(patientNumber, "Generated patient number successfully.");
        }

        /// <summary>
        /// Assign patient numbers to all existing patients who don’t have one
        /// </summary>
        [HttpPost("assign-legacy-numbers")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignLegacyNumbers(CancellationToken ct = default)
        {
            await _numberGenerator.AssignNumbersToExistingPatientsAsync(ct);
            return Success("Patient numbers assigned to legacy patients successfully.");
        }

        /// <summary>
        /// Get camp overview for a patient (camps attended and upcoming). Use patientId from /auth/me (relatedEntityId when relatedEntityType is Patient).
        /// </summary>
        [HttpGet("{patientId:guid}/camp-overview")]
        [Authorize(Roles = "Admin,Patient")]
        [ProducesResponseType(typeof(ApiResponse<PatientCampOverviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPatientCampOverview(Guid patientId, CancellationToken ct = default)
        {
            var result = await _campOverviewService.GetPatientCampOverviewAsync(patientId, ct);
            return Success(result);
        }
    }
}

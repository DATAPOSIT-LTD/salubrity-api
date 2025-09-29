using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.Interfaces.Services.Camps;
using Salubrity.Application.Interfaces.Services.Concierge;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Concierge
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/concierge")]
    [Produces("application/json")]
    [Tags("Concierge Management")]
    public class ConciergeController : BaseController
    {
        private readonly IConciergeService _service;
        public ConciergeController(IConciergeService service)
        {
            _service = service;
        }

        [HttpGet("{campId:guid}/service-stations-info")]
        [ProducesResponseType(typeof(ApiResponse<List<CampServiceStationInfoDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampServiceStationsInfo(Guid campId, CancellationToken ct)
        {
            var stations = await _service.GetCampServiceStationsAsync(campId, ct);
            return Success(stations);
        }

        [HttpGet("{campId:guid}/queue-priorities")]
        [ProducesResponseType(typeof(ApiResponse<List<CampQueuePriorityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampQueuePriorities(Guid campId, CancellationToken ct)
        {
            var priorities = await _service.GetCampQueuePrioritiesAsync(campId, ct);
            return Success(priorities);
        }

        [HttpGet("camps/{campId:guid}/stations-queue")]
        [ProducesResponseType(typeof(ApiResponse<List<CampServiceStationWithQueueDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampServiceStationsWithQueue(Guid campId, CancellationToken ct)
        {
            var result = await _service.GetCampServiceStationsWithQueueAsync(campId, ct);
            return Success(result);
        }

        [HttpGet("{patientId:guid}/detail")]
        [ProducesResponseType(typeof(ApiResponse<PatientDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPatientDetail(Guid patientId, CancellationToken ct)
        {
            var result = await _service.GetPatientDetailByIdAsync(patientId, ct);

            //if (result == null)
            //{
            //    return Failure("Patient not found.");
            //}    

            return Success(result);
        }
    }
}

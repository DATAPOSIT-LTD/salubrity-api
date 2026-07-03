using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.Concierge;
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
        public ConciergeController(IConciergeService service) => _service = service;

        [HttpGet("{campId:guid}/service-stations-info")]
        [ProducesResponseType(typeof(ApiResponse<List<CampServiceStationInfoDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampServiceStationsInfo(Guid campId, CancellationToken ct)
            => Success(await _service.GetCampServiceStationsAsync(campId, ct));

        [HttpGet("{campId:guid}/queue-priorities")]
        [ProducesResponseType(typeof(ApiResponse<List<CampQueuePriorityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampQueuePriorities(Guid campId, CancellationToken ct)
            => Success(await _service.GetCampQueuePrioritiesAsync(campId, ct));

        [HttpGet("camps/{campId:guid}/stations-queue")]
        [ProducesResponseType(typeof(ApiResponse<List<CampServiceStationWithQueueDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampServiceStationsWithQueue(Guid campId, CancellationToken ct)
            => Success(await _service.GetCampServiceStationsWithQueueAsync(campId, ct));

        [HttpGet("{patientId:guid}/detail")]
        [ProducesResponseType(typeof(ApiResponse<PatientDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPatientDetail(Guid patientId, CancellationToken ct)
            => Success(await _service.GetPatientDetailByIdAsync(patientId, ct));

        [HttpGet("participants/{participantId:guid}/stations")]
        [ProducesResponseType(typeof(ApiResponse<List<ParticipantStationStatusDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetParticipantStations(Guid participantId, CancellationToken ct)
            => Success(await _service.GetParticipantStationsAsync(participantId, ct));

        [HttpGet("{campId:guid}/live-stats")]
        [ProducesResponseType(typeof(ApiResponse<CampLiveStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampLiveStats(Guid campId, CancellationToken ct)
            => Success(await _service.GetCampLiveStatsAsync(campId, ct));

        [HttpGet("{campId:guid}/participants/search")]
        [ProducesResponseType(typeof(ApiResponse<List<ParticipantSearchResultDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchParticipants(Guid campId, [FromQuery] string q, CancellationToken ct)
            => Success(await _service.SearchParticipantsAsync(campId, q ?? string.Empty, ct));

        [HttpGet("{campId:guid}/station-bottlenecks")]
        [ProducesResponseType(typeof(ApiResponse<List<StationBottleneckDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStationBottlenecks(Guid campId, CancellationToken ct)
            => Success(await _service.GetStationBottlenecksAsync(campId, ct));

        [HttpGet("{campId:guid}/registration-timeline")]
        [ProducesResponseType(typeof(ApiResponse<RegistrationTimelineDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRegistrationTimeline(Guid campId, CancellationToken ct)
            => Success(await _service.GetRegistrationTimelineAsync(campId, ct));
    }
}

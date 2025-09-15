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
    }
}

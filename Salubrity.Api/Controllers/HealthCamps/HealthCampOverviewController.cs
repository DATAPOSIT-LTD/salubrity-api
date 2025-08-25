using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.HealthCamps
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/health-camp-overview")]
    [Produces("application/json")]
    [Tags("Health Camp Overview")]
    public class HealthCampOverviewController : BaseController
    {
        private readonly IHealthCampOverviewService _service;

        public HealthCampOverviewController(IHealthCampOverviewService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<HealthCampOverviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHealthCampOverview()
        {
            var result = await _service.GetHealthCampOverviewAsync();
            return Success(result);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.DTOs.HomepageOverview;
using Salubrity.Application.Interfaces.Services.HomepageOverview;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.HomepageOverview
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/overview")]
    [Produces("application/json")]
    [Tags("Homepage Overview")]
    public class HomepageOverviewController : BaseController
    {
        private readonly IHomepageOverviewService _overviewService;

        public HomepageOverviewController(IHomepageOverviewService overviewService)
        {
            _overviewService = overviewService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<HomepageOverviewDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]

        public async Task<IActionResult> GetHomepageOverview()
        {
            var result = await _overviewService.GetHomepageOverviewAsync();
            return Success(result);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Salubrity.Application.DTOs.HomepageOverview;
using Salubrity.Application.Interfaces.Services.HomepageOverview;

namespace Salubrity.Api.Controllers.HomepageOverview
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/overview")]
    [Produces("application/json")]
    [Tags("Homepage Overview")]
    public class HomepageOverviewController : ControllerBase
    {
        private readonly IHomepageOverviewService _overviewService;

        public HomepageOverviewController(IHomepageOverviewService overviewService)
        {
            _overviewService = overviewService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(HomepageOverviewDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHomepageOverview()
        {
            var result = await _overviewService.GetHomepageOverviewAsync();
            return Ok(result);
        }
    }
}

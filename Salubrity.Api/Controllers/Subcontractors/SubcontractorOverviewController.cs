using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Application.Interfaces.Services.Subcontractors;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Subcontractors
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/subcontractor-overview")]
    [Produces("application/json")]
    [Tags("Subcontractor Overview")]
    public class SubcontractorOverviewController : BaseController
    {
        private readonly ISubcontractorOverviewService _service;

        public SubcontractorOverviewController(ISubcontractorOverviewService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<SubcontractorOverviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubcontractorOverview()
        {
            var result = await _service.GetSubcontractorOverviewAsync();
            return Success(result);
        }
    }
}

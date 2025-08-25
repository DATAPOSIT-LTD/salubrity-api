using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Organizations;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Organizations
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/organization-overview")]
    [Produces("application/json")]
    [Tags("Organization Overview")]
    public class OrganizationOverviewController : BaseController
    {
        private readonly IOrganizationOverviewService _service;

        public OrganizationOverviewController(IOrganizationOverviewService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<OrganizationOverviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrganizationOverview()
        {
            var result = await _service.GetOrganizationOverviewAsync();
            return Success(result);
        }
    }
}

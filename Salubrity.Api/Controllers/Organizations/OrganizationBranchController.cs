using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Organizations.Branches;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Organizations
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/organizations")]
    [Produces("application/json")]
    [Tags("Organization Branches")]
    public class OrganizationBranchController : BaseController
    {
        private readonly IOrganizationBranchService _service;

        public OrganizationBranchController(IOrganizationBranchService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // CREATE BRANCH
        // POST /organizations/{orgId}/branches
        // ─────────────────────────────────────────────
        [HttpPost("{orgId:guid}/branches")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationBranchResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateBranch(
            Guid orgId,
            [FromBody] OrganizationBranchCreateDto dto,
            CancellationToken ct = default)
        {
            var result = await _service.CreateAsync(orgId, dto);
            return Success(result);
        }

        // ─────────────────────────────────────────────
        // GET ALL BRANCHES FOR AN ORGANIZATION
        // GET /organizations/{orgId}/branches
        // ─────────────────────────────────────────────
        [HttpGet("{orgId:guid}/branches")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrganizationBranchResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranches(Guid orgId, CancellationToken ct = default)
        {
            var result = await _service.GetForOrganizationAsync(orgId);
            return Success(result);
        }

        // ─────────────────────────────────────────────
        // GET SINGLE BRANCH
        // GET /organizations/branches/{branchId}
        // ─────────────────────────────────────────────
        [HttpGet("branches/{branchId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationBranchResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranchById(Guid branchId, CancellationToken ct = default)
        {
            var result = await _service.GetByIdAsync(branchId);
            return Success(result);
        }

        // ─────────────────────────────────────────────
        // UPDATE BRANCH
        // PUT /organizations/branches/{branchId}
        // ─────────────────────────────────────────────
        [HttpPut("branches/{branchId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateBranch(
            Guid branchId,
            [FromBody] OrganizationBranchUpdateDto dto,
            CancellationToken ct = default)
        {
            await _service.UpdateAsync(branchId, dto);
            return Success("Branch updated successfully.");
        }

        // ─────────────────────────────────────────────
        // DELETE BRANCH
        // DELETE /organizations/branches/{branchId}
        // ─────────────────────────────────────────────
        [HttpDelete("branches/{branchId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteBranch(Guid branchId, CancellationToken ct = default)
        {
            await _service.DeleteAsync(branchId);
            return Success("Branch deleted successfully.");
        }

        // ─────────────────────────────────────────────
        // BULK UPLOAD BRANCHES
        // POST /organizations/{orgId}/branches/bulk-upload
        // ─────────────────────────────────────────────
        [HttpPost("{orgId:guid}/branches/bulk-upload")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BulkUploadBranches(
            Guid orgId,
            IFormFile file,
            CancellationToken ct = default)
        {
            await _service.BulkUploadAsync(orgId, file);
            return Success("Branch bulk upload initiated.");
        }
    }
}

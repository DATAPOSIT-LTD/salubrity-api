using Microsoft.AspNetCore.Mvc;
using Salubrity.Domain.Seeders;
using Salubrity.Shared.Responses;
using Salubrity.Api.Controllers.Common;

namespace Salubrity.Api.Controllers.Rbac;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rbac")]
[Produces("application/json")]
[Tags("RBAC Seeder")]
public class RbacController : BaseController
{
    private readonly IRbacSeeder _rbacSeeder;

    public RbacController(IRbacSeeder rbacSeeder)
    {
        _rbacSeeder = rbacSeeder;
    }

    [HttpPost("seed")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SeedDefaults(CancellationToken cancellationToken)
    {
        await _rbacSeeder.SeedDefaultRolesAsync(cancellationToken);
        return Success("RBAC defaults seeded successfully.");
    }
}

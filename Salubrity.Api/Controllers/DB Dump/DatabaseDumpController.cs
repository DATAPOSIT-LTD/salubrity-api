using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.DB_Dump;
using Salubrity.Application.Interfaces.Repositories.DB_Dump;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.DB_Dump
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/db/dump")]
    [Produces("application/json")]
    [Tags("Database Dump Management")]
    public class DatabaseDumpController : BaseController
    {
        private readonly IDatabaseDumpRepository _repo;
        private readonly DatabaseDumpOptions _options;


        public DatabaseDumpController(IDatabaseDumpRepository repo, IOptions<DatabaseDumpOptions> options)
        {
            _repo = repo;
            _options = options.Value;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponse<DatabaseDumpOptions>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create()
        {
            var result = await _repo.CreateDumpAsync();
            return Success(new { FilePath = result });
        }

        [HttpGet("download")]
        public IActionResult Download([FromQuery] string fileName)
        {
            var filePath = Path.Combine(_options.Directory, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, fileName);
        }
    }
}

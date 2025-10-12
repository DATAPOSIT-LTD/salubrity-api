using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using Salubrity.Application.DTOs.DB_Dump;
using Salubrity.Application.Interfaces.Repositories.DB_Dump;
using System.Diagnostics;

namespace Salubrity.Infrastructure.Repositories.DB_Dump
{
    public class DatabaseDumpRepository : IDatabaseDumpRepository
    {
        private readonly DatabaseDumpOptions _options;
        private readonly string _connectionString;

        public DatabaseDumpRepository(
            IOptions<DatabaseDumpOptions> options,
            IConfiguration configuration)
        {
            _options = options.Value;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<string> CreateDumpAsync()
        {
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            var database = builder.Database;
            var host = builder.Host;
            var port = builder.Port;
            var username = builder.Username;
            var password = builder.Password;

            var fileName = $"Salubrity_dump_{DateTime.UtcNow.AddHours(3):yyyyMMdd_HHmmss}.sql";
            var filePath = Path.Combine(_options.Directory, fileName);

            if (string.IsNullOrWhiteSpace(_options.Directory))
                throw new InvalidOperationException("Database dump directory is not configured. Please set DatabaseDump:Directory in your configuration.");

            Directory.CreateDirectory(_options.Directory);

            // Build the pg_dump command
            var args = $"-h {host} -p {port} -U {username} -F p -d {database} -f \"{filePath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            // Pass the password via environment variable
            startInfo.Environment["PGPASSWORD"] = password;

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            string stdOut = await process.StandardOutput.ReadToEndAsync();
            string stdErr = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"pg_dump failed: {stdErr}");
            }

            return filePath;
        }
    }
}

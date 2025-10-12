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
        private readonly string _connectionString;

        public DatabaseDumpRepository(
            IOptions<DatabaseDumpOptions> options, // required for DI, not used
            IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<string> CreateDumpAsync()
        {
            // Use a folder called "dbdumps" in the current working directory
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "dbdumps");

            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Could not create or access the dump directory '{directory}'. " +
                    $"Error: {ex.Message}. " +
                    $"Make sure the application has write permissions to its own folder.");
            }

            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            var database = builder.Database;
            var host = builder.Host;
            var port = builder.Port;
            var username = builder.Username;
            var password = builder.Password;

            var fileName = $"Salubrity_{DateTime.UtcNow.AddHours(3):yyyyMMdd_HHmmss}.sql";
            var filePath = Path.Combine(directory, fileName);

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

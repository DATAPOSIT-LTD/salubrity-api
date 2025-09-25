using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Salubrity.Application.Interfaces.Repositories.Users;

namespace Salubrity.Application.Services.Jobs
{
    public class RelatedEntityBackfillJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RelatedEntityBackfillJob> _logger;

        public RelatedEntityBackfillJob(IServiceScopeFactory scopeFactory, ILogger<RelatedEntityBackfillJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var subcontractorRows = await repo.BackfillSubcontractorLinksAsync(stoppingToken);
                    var patientRows = await repo.BackfillPatientLinksAsync(stoppingToken);

                    var total = subcontractorRows + patientRows;

                    if (total > 0)
                    {
                        _logger.LogInformation(
                            "Backfill completed at {time}: {sub} subcontractors, {pat} patients updated.",
                            DateTime.UtcNow, subcontractorRows, patientRows
                        );
                    }
                    else
                    {
                        _logger.LogInformation("Backfill completed at {time}: no changes needed.", DateTime.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during RelatedEntity backfill job");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}

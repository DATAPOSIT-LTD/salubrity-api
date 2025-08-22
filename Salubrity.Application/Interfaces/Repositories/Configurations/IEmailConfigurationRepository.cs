// File: Salubrity.Application.Interfaces.Repositories.Configurations/IEmailConfigurationRepository.cs

using Salubrity.Domain.Entities.Configurations;

public interface IEmailConfigurationRepository
{
    Task<EmailConfiguration?> GetActiveAsync(CancellationToken ct = default);
}

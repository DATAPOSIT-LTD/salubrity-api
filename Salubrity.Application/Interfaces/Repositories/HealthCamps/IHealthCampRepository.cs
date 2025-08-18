using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampRepository
{
    Task<List<HealthCampListDto>> GetAllAsync();
    Task<HealthCamp?> GetByIdAsync(Guid id);
    Task<HealthCampDetailDto?> GetCampDetailsByIdAsync(Guid id);
    Task<HealthCamp> CreateAsync(HealthCamp entity);
    Task<HealthCamp> UpdateAsync(HealthCamp entity);
    Task DeleteAsync(Guid id);

    Task<HealthCamp?> GetForLaunchAsync(Guid id);
    Task UpsertTempCredentialAsync(HealthCampTempCredentialUpsert upsert);
    Task SaveChangesAsync();
}

public sealed class HealthCampTempCredentialUpsert
{
    public Guid HealthCampId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = null!;
    public string TempPasswordHash { get; set; } = null!;
    public DateTimeOffset TempPasswordExpiresAt { get; set; }
    public string SignInJti { get; set; } = null!;
    public DateTimeOffset TokenExpiresAt { get; set; }
}

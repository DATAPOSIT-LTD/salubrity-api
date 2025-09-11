using Salubrity.Infrastructure.Persistence;
using Salubrity.Infrastructure.Seeders;

namespace Salubrity.Infrastructure;

/// <summary>
/// Orchestrates seeding of all system data.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await InsuranceProviderSeeder.SeedAsync(db);
        await BillingStatusSeeder.SeedAsync(db);
        // TODO: Add more seeders here as needed
        // await GenderSeeder.SeedAsync(db);
        // await OrganizationStatusSeeder.SeedAsync(db);
    }
}

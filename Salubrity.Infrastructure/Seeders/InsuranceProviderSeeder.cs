using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Seeders;

public static class InsuranceProviderSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.InsuranceProviders.AnyAsync())
        {
            var providers = new List<InsuranceProvider>
            {
                new()
                {
                    Name = "AAR Insurance",
                    Description = "AAR Health Cover",
                    LogoUrl = "https://cdn.salubrity.com/logos/insurance/aar.svg"
                },
                new()
                {
                    Name = "Jubilee Insurance",
                    Description = "Jubilee Kenya Medical",
                    LogoUrl = "https://cdn.salubrity.com/logos/insurance/jubilee.svg"
                },
                new()
                {
                    Name = "NHIF",
                    Description = "National Hospital Insurance Fund",
                    LogoUrl = "https://cdn.salubrity.com/logos/insurance/nhif.svg"
                },
                new()
                {
                    Name = "CIC Insurance",
                    Description = "CIC Health Services",
                    LogoUrl = "https://cdn.salubrity.com/logos/insurance/cic.svg"
                }
            };

            db.InsuranceProviders.AddRange(providers);
            await db.SaveChangesAsync();
        }
    }
}

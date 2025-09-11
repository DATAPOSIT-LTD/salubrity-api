using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Seeders
{
    public static class BillingStatusSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (!await db.Set<BillingStatus>().AnyAsync())
            {
                var statuses = new List<BillingStatus>
            {
                new() { Name = "Billed" },
                new() { Name = "Not Billed" },
                new() { Name = "Proceed without billing" }
            };

                await db.Set<BillingStatus>().AddRangeAsync(statuses);
                await db.SaveChangesAsync();
            }
        }
    }
}

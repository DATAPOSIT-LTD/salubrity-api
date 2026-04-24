using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Seeders
{
    public static class UrgencySeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (!await db.Set<Urgency>().AnyAsync())
            {
                var items = new List<Urgency>
                {
                    new() { Name = "Low" },
                    new() { Name = "Medium" },
                    new() { Name = "High" },
                };
                await db.Set<Urgency>().AddRangeAsync(items);
                await db.SaveChangesAsync();
            }
        }
    }
}

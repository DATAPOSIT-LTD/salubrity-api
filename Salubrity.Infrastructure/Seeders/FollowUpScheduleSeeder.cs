using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Seeders
{
    public static class FollowUpScheduleSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (!await db.Set<FollowUpSchedule>().AnyAsync())
            {
                var items = new List<FollowUpSchedule>
                {
                    new() { Name = "1 Week" },
                    new() { Name = "1 Month" },
                    new() { Name = "3 Months" },
                    new() { Name = "6 Months" },
                    new() { Name = "1 Year" },
                };
                await db.Set<FollowUpSchedule>().AddRangeAsync(items);
                await db.SaveChangesAsync();
            }
        }
    }
}

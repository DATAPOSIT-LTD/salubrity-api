using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Seeders;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Seeders;

public class RbacSeeder : IRbacSeeder
{
    private readonly AppDbContext _context;

    public RbacSeeder(AppDbContext context)
    {
        _context = context;
    }


    public async Task SeedDefaultRolesAsync(CancellationToken cancellationToken = default)
    {
        if (!await _context.Roles.AnyAsync(cancellationToken))
        {
            _context.Roles.AddRange(new[]
            {
                new Role { Name = "System Admin", Description = "System Admin" },
                new Role { Name = "Subcontractor", Description = "Service Provider" },
                new Role { Name = "Employee", Description = "Employee User" }
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

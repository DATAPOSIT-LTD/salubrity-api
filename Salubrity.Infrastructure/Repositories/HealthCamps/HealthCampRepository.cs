using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Infrastructure.Repositories.HealthCamps;

public class HealthCampRepository : IHealthCampRepository
{
    private readonly AppDbContext _context;
    private readonly IPackageReferenceResolver _referenceResolver;

    public HealthCampRepository(AppDbContext context, IPackageReferenceResolver referenceResolver)
    {
        _context = context;
        _referenceResolver = referenceResolver;
    }

    public async Task<List<HealthCampListDto>> GetAllAsync()
    {
        var camps = await _context.HealthCamps
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Include(c => c.Organization)
            .Include(c => c.ServicePackage)     //  load chosen package
            .Include(c => c.ServiceAssignments) // for counts/status
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var list = new List<HealthCampListDto>(camps.Count);

        foreach (var c in camps)
        {
            list.Add(new HealthCampListDto
            {
                Id = c.Id,
                ClientName = c.Organization?.BusinessName ?? "N/A",
                ExpectedPatients = c.ExpectedParticipants ?? 0,
                Venue = c.Location ?? "N/A",
                DateRange = $"{c.StartDate:dd} - {c.EndDate:dd MMM, yyyy}",
                SubcontractorCount = c.ServiceAssignments?.Count ?? 0,
                Status = (c.ServiceAssignments?.Any() ?? false) ? "Ready" : "Incomplete",
                PackageName = c.ServicePackage?.Name ?? "N/A"   //  chosen package only
            });
        }

        return list;
    }


    public async Task<HealthCampDetailDto?> GetCampDetailsByIdAsync(Guid id)
    {
        var camp = await _context.HealthCamps
            .AsNoTracking()
            .Include(c => c.Organization)
                .ThenInclude(o => o.InsuranceProviders)
                    .ThenInclude(ip => ip.InsuranceProvider)
            .Include(c => c.ServicePackage)   // <-- chosen package
            .Include(c => c.PackageItems)     // <-- items (services/categories)
            .Include(c => c.ServiceAssignments)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (camp is null) return null;

        var dto = new HealthCampDetailDto
        {
            Id = camp.Id,
            Name = camp.Name,
            StartDate = camp.StartDate,
            ClientName = camp.Organization?.BusinessName ?? "N/A",
            Venue = camp.Location ?? "N/A",
            ExpectedPatients = camp.ExpectedParticipants ?? 0,
            SubcontractorCount = camp.ServiceAssignments?.Count ?? 0,
            // chosen package only
            PackageName = camp.ServicePackage?.Name ?? "N/A",
            PackageCost = camp.ServicePackage?.Price,
            InsurerName = camp.Organization?.InsuranceProviders?.FirstOrDefault()?.InsuranceProvider?.Name ?? string.Empty,
            ServiceStations = new List<ServiceStationDto>()
        };

        // Resolve station names (services/categories) concurrently
        var itemTasks = camp.PackageItems
            .Where(pi => !pi.IsDeleted)
            .Select(async pi =>
            {
                var name = await _referenceResolver.GetNameAsync(pi.ReferenceType, pi.ReferenceId);
                return new ServiceStationDto
                {
                    Id = pi.Id,
                    Name = name,
                    PatientsServed = 24,     // TODO: replace with real metrics
                    PendingService = 35,     // TODO: replace with real metrics
                    AvgTimePerPatient = "3 min",
                    OutlierAlerts = 0
                };
            });

        dto.ServiceStations.AddRange(await Task.WhenAll(itemTasks));

        return dto;
    }

    public async Task<HealthCamp?> GetByIdAsync(Guid id)
    {
        return await _context.HealthCamps
            .Include(c => c.PackageItems)
            .Include(c => c.ServiceAssignments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<HealthCamp> CreateAsync(HealthCamp entity)
    {
        _context.HealthCamps.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<HealthCamp> UpdateAsync(HealthCamp entity)
    {
        _context.HealthCamps.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var camp = await _context.HealthCamps.FindAsync(id);
        if (camp != null)
        {
            camp.IsDeleted = true;
            camp.DeletedAt = DateTime.UtcNow;
            _context.HealthCamps.Update(camp);
            await _context.SaveChangesAsync();
        }
    }


}

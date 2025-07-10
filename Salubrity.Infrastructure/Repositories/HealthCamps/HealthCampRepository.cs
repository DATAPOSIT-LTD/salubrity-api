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
            .Include(c => c.Organization)
            .Include(c => c.PackageItems)
            .Include(c => c.ServiceAssignments)
            .ToListAsync();

        var list = new List<HealthCampListDto>();

        foreach (var camp in camps)
        {
            var dto = new HealthCampListDto
            {
                Id = camp.Id,
                ClientName = camp.Organization?.BusinessName ?? "N/A",
                ExpectedPatients = 80, // TODO: Replace with camp.ExpectedPatients
                Venue = camp.Location??"N/A",
                DateRange = $"{camp.StartDate:dd} - {camp.EndDate:dd MMM, yyyy}",
                SubcontractorCount = camp.ServiceAssignments?.Count ?? 0,
                Status = (camp.ServiceAssignments?.Any() ?? false) ? "Ready" : "Incomplete",
                PackageName = string.Empty
            };

            var firstItem = camp.PackageItems.FirstOrDefault();
            if (firstItem != null)
            {
                dto.PackageName = await _referenceResolver.GetNameAsync(firstItem.ReferenceType, firstItem.ReferenceId);
            }

            list.Add(dto);
        }

        return list;
    }

    public async Task<HealthCampDetailDto?> GetCampDetailsByIdAsync(Guid id)
    {
        var camp = await _context.HealthCamps
            .Include(c => c.Organization)
                .ThenInclude(o => o.InsuranceProviders)
                    .ThenInclude(ip => ip.InsuranceProvider)
            .Include(c => c.PackageItems)
                .ThenInclude(pi => pi.ReferenceId)
            .Include(c => c.ServiceAssignments)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (camp == null) return null;

        var dto = new HealthCampDetailDto
        {
            Id = camp.Id,
            Name = camp.Name,
            StartDate = camp.StartDate,
            ClientName = camp.Organization?.BusinessName ?? "N/A",
            Venue = camp.Location,
            ExpectedPatients = 80, // TODO: Replace with camp.ExpectedPatients
            SubcontractorCount = camp.ServiceAssignments?.Count ?? 0,
            PackageName = string.Empty,
            PackageCost = null,
            InsurerName = string.Empty,
            ServiceStations = new List<ServiceStationDto>()
        };

        var firstItem = camp.PackageItems.FirstOrDefault();
        if (firstItem != null)
        {
            dto.PackageName = await _referenceResolver.GetNameAsync(firstItem.ReferenceType, firstItem.ReferenceId);
            dto.PackageCost = 0;//firstItem.ServicePackage?.Price;
        }

        var insurer = camp.Organization?.InsuranceProviders?.FirstOrDefault();
        if (insurer != null)
        {
            dto.InsurerName = insurer.InsuranceProvider?.Name ?? string.Empty;
        }

        foreach (var pi in camp.PackageItems)
        {
            var name = await _referenceResolver.GetNameAsync(pi.ReferenceType, pi.ReferenceId);

            dto.ServiceStations.Add(new ServiceStationDto
            {
                Id = pi.Id,
                Name = name,
                PatientsServed = 24,
                PendingService = 35,
                AvgTimePerPatient = "3 min",
                OutlierAlerts = 0
            });
        }

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

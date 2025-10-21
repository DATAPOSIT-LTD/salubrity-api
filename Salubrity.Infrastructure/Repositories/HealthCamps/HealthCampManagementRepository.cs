using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;
using System.Text.Json.Serialization;

namespace Salubrity.Infrastructure.Repositories.HealthCamps;

public class HealthCampManagementRepository : IHealthCampManagementRepository
{
	private readonly AppDbContext _context;
	private readonly IPackageReferenceResolver _referenceResolver;

	public HealthCampManagementRepository(AppDbContext context, IPackageReferenceResolver referenceResolver)
	{
		_context = context;
		_referenceResolver = referenceResolver;
	}



	// public async Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId, bool group = false)
	// {
	// 	var packageItems = await _context.HealthCampPackageItems
	// 		.Where(x => x.HealthCampId == healthCampId)
	// 		.ToListAsync();

	// 	var assignments = await _context.HealthCampServiceAssignments
	// 		.Where(x => x.HealthCampId == healthCampId)
	// 		.Include(x => x.Subcontractor)
	// 		.ToListAsync();

	// 	var userIds = assignments
	// 		.Select(a => a.Subcontractor.UserId)
	// 		.Distinct()
	// 		.ToList();

	// 	var users = await _context.Users
	// 		.Where(u => userIds.Contains(u.Id))
	// 		.ToDictionaryAsync(
	// 			u => u.Id,
	// 			u => string.Join(" ", new[] { u.FirstName, u.MiddleName, u.LastName }
	// 				.Where(n => !string.IsNullOrWhiteSpace(n)))
	// 		);

	// 	// Step 1: build flat DTOs
	// 	var flatList = new List<ServiceStationSummaryDto>();
	// 	foreach (var item in packageItems)
	// 	{
	// 		var type = item.ReferenceType;
	// 		var name = await _referenceResolver.GetNameAsync(type, item.ReferenceId);

	// 		var staffNames = assignments
	// 			.Where(a =>
	// 				a.AssignmentId == item.ReferenceId &&
	// 				(PackageItemType)a.AssignmentType == type)
	// 			.Select(a => a.Subcontractor.UserId)
	// 			.Where(userId => users.ContainsKey(userId))
	// 			.Select(userId => users[userId])
	// 			.Distinct()
	// 			.ToList();

	// 		flatList.Add(new ServiceStationSummaryDto
	// 		{
	// 			Id = item.ReferenceId,
	// 			Type = type,
	// 			Name = name,
	// 			Staff = staffNames,
	// 			PatientsServed = 0,
	// 			PendingPatients = 0,
	// 			AverageTimePerPatient = "0 min",
	// 			OutlierAlerts = 0
	// 		});
	// 	}

	// 	if (!group)
	// 		return flatList;

	// 	// Step 2: nest using resolver
	// 	var lookup = flatList.ToDictionary(x => x.Id, x => x);
	// 	var groupedList = new List<ServiceStationSummaryDto>();

	// 	foreach (var station in flatList)
	// 	{
	// 		var (parentId, _) = await _referenceResolver.GetParentAsync(station.Id);

	// 		if (parentId != null && lookup.ContainsKey(parentId.Value))
	// 		{
	// 			lookup[parentId.Value].Children.Add(station);
	// 		}
	// 		else
	// 		{
	// 			groupedList.Add(station); // top-level (service)
	// 		}
	// 	}

	// 	return groupedList;
	// }
	public async Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId, bool group = false)
	{
		// 🧩 Get all active packages for this camp
		var campPackages = await _context.HealthCampPackages
			.Include(p => p.ServicePackage)
			.Where(p => p.HealthCampId == healthCampId && p.IsActive)
			.ToListAsync();

		// 🧩 Fetch package items (services/categories) scoped by active packages
		var packageIds = campPackages.Select(p => p.ServicePackageId).ToList();

		var packageItems = await _context.HealthCampPackageItems
			.Where(x => x.HealthCampId == healthCampId && packageIds.Contains(x.ReferenceId))
			.ToListAsync();

		// 🧩 Fetch service assignments for this camp
		var assignments = await _context.HealthCampServiceAssignments
			.Where(x => x.HealthCampId == healthCampId)
			.Include(x => x.Subcontractor)
			.ToListAsync();

		// 🧩 Build user dictionary
		var userIds = assignments
			.Select(a => a.Subcontractor.UserId)
			.Distinct()
			.ToList();

		var users = await _context.Users
			.Where(u => userIds.Contains(u.Id))
			.ToDictionaryAsync(
				u => u.Id,
				u => string.Join(" ", new[] { u.FirstName, u.MiddleName, u.LastName }
					.Where(n => !string.IsNullOrWhiteSpace(n)))
			);

		// 🧾 Step 1: build flat DTOs
		var flatList = new List<ServiceStationSummaryDto>();

		foreach (var item in packageItems)
		{
			var type = item.ReferenceType;
			var name = await _referenceResolver.GetNameAsync(type, item.ReferenceId);

			var staffNames = assignments
				.Where(a => a.AssignmentId == item.ReferenceId &&
							(PackageItemType)a.AssignmentType == type)
				.Select(a => a.Subcontractor.UserId)
				.Where(userId => users.ContainsKey(userId))
				.Select(userId => users[userId])
				.Distinct()
				.ToList();

			// 🔗 Find which package this service belongs to
			var package = campPackages.FirstOrDefault(p => p.ServicePackageId == item.ReferenceId);

			flatList.Add(new ServiceStationSummaryDto
			{
				Id = item.ReferenceId,
				Type = type,
				Name = name,
				PackageId = package?.Id,
				PackageName = package?.ServicePackage?.Name ?? package?.DisplayName,
				Staff = staffNames,
				PatientsServed = 0,
				PendingPatients = 0,
				AverageTimePerPatient = "0 min",
				OutlierAlerts = 0
			});
		}

		if (!group)
			return flatList;

		// 🧾 Step 2: nest using resolver (if requested)
		var lookup = flatList.ToDictionary(x => x.Id, x => x);
		var groupedList = new List<ServiceStationSummaryDto>();

		foreach (var station in flatList)
		{
			var (parentId, _) = await _referenceResolver.GetParentAsync(station.Id);

			if (parentId != null && lookup.ContainsKey(parentId.Value))
			{
				lookup[parentId.Value].Children.Add(station);
			}
			else
			{
				groupedList.Add(station);
			}
		}

		return groupedList;
	}

	public async Task<List<CampPatientSummaryDto>> GetCampPatientsAsync(Guid healthCampId)
	{
		// Assuming there's a CampPatient or HealthCampPatient table (not shown in your model)
		return new(); // Stubbed
	}

	public async Task<List<CampDailyActivityDto>> GetActivitySummaryAsync(Guid healthCampId)
	{
		// This could be derived from logs if implemented
		return new(); // Stubbed
	}

	public async Task<List<CampBillingSummaryDto>> GetBillingSummaryAsync(Guid healthCampId)
	{
		// Assuming billing/invoice relationship via patients or services
		return new(); // Stubbed
	}

	public async Task<HealthCamp?> GetWithDetailsAsync(Guid id)
	{
		return await _context.HealthCamps
			.Include(h => h.Organization)
				.ThenInclude(o => o.InsuranceProviders)
					.ThenInclude(oi => oi.InsuranceProvider)
			.Include(h => h.Organization)
				.ThenInclude(o => o.Packages)
					.ThenInclude(op => op.ServicePackage)
			.Include(h => h.ServiceAssignments)
			.Include(h => h.PackageItems)
			.FirstOrDefaultAsync(h => h.Id == id);
	}
}

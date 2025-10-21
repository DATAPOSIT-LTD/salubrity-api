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
	// public async Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId, bool group = false)
	// {
	// 	// 🧩 Get all active packages for this camp
	// 	var campPackages = await _context.HealthCampPackages
	// 		.Include(p => p.ServicePackage)
	// 		.Where(p => p.HealthCampId == healthCampId && p.IsActive)
	// 		.ToListAsync();

	// 	// 🧩 Fetch package items (services/categories) scoped by camp
	// 	var packageItems = await _context.HealthCampPackageItems
	// 		.Where(x => x.HealthCampId == healthCampId)
	// 		.ToListAsync();

	// 	// 🧩 Fetch service assignments for this camp
	// 	var assignments = await _context.HealthCampServiceAssignments
	// 		.Where(x => x.HealthCampId == healthCampId)
	// 		.Include(x => x.Subcontractor)
	// 		.ToListAsync();

	// 	// 🧩 Build user dictionary
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

	// 	// 🧾 Step 1: build flat DTOs
	// 	var flatList = new List<ServiceStationSummaryDto>();

	// 	foreach (var item in packageItems)
	// 	{
	// 		var type = item.ReferenceType;
	// 		var name = await _referenceResolver.GetNameAsync(type, item.ReferenceId) ?? "Unknown";

	// 		var staffNames = assignments
	// 			.Where(a => a.AssignmentId == item.ReferenceId &&
	// 						(PackageItemType)a.AssignmentType == type)
	// 			.Select(a => a.Subcontractor.UserId)
	// 			.Where(userId => users.ContainsKey(userId))
	// 			.Select(userId => users[userId])
	// 			.Distinct()
	// 			.ToList();

	// 		// 🔗 Correct package mapping (HealthCampPackageId, not ServicePackageId)
	// 		var package = campPackages.FirstOrDefault(p => p.Id == item.HealthCampId);

	// 		flatList.Add(new ServiceStationSummaryDto
	// 		{
	// 			Id = item.ReferenceId,
	// 			Type = type,
	// 			Name = name,
	// 			PackageId = package?.Id,
	// 			PackageName = package?.ServicePackage?.Name ?? package?.DisplayName,
	// 			Staff = staffNames,
	// 			PatientsServed = 0,
	// 			PendingPatients = 0,
	// 			AverageTimePerPatient = "0 min",
	// 			OutlierAlerts = 0
	// 		});
	// 	}

	// 	if (!group)
	// 		return flatList;

	// 	// 🧾 Step 2: nest using resolver (if requested)
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
	// 			groupedList.Add(station);
	// 		}
	// 	}

	// 	return groupedList;
	// }

	// public async Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId, bool group = false)
	// {
	// 	// 1️⃣ Get active packages for this camp
	// 	var campPackages = await _context.HealthCampPackages
	// 		.Include(p => p.ServicePackage)
	// 		.Where(p => p.HealthCampId == healthCampId && p.IsActive)
	// 		.ToListAsync();

	// 	if (!campPackages.Any())
	// 		return new List<ServiceStationSummaryDto>();

	// 	// 2️⃣ Get all package items within this camp (these define which services belong)
	// 	var campPackageItems = await _context.HealthCampPackageItems
	// 		.Where(x => x.HealthCampId == healthCampId)
	// 		.ToListAsync();

	// 	// Build quick lookup for valid service IDs/types
	// 	var validServiceKeys = campPackageItems
	// 		.Select(i => new { i.ReferenceId, i.ReferenceType })
	// 		.ToHashSet();

	// 	// 3️⃣ Fetch all service assignments for this camp
	// 	var assignments = await _context.HealthCampServiceAssignments
	// 		.Where(a => a.HealthCampId == healthCampId)
	// 		.Include(a => a.Subcontractor)
	// 		.ToListAsync();

	// 	// 4️⃣ Build user dictionary (Subcontractor → Name)
	// 	var userIds = assignments
	// 		.Where(a => a.Subcontractor != null)
	// 		.Select(a => a.Subcontractor!.UserId)
	// 		.Distinct()
	// 		.ToList();

	// 	var users = userIds.Any()
	// 		? await _context.Users
	// 			.Where(u => userIds.Contains(u.Id))
	// 			.ToDictionaryAsync(
	// 				u => u.Id,
	// 				u => string.Join(" ", new[] { u.FirstName, u.MiddleName, u.LastName }
	// 					.Where(n => !string.IsNullOrWhiteSpace(n)))
	// 			)
	// 		: new Dictionary<Guid, string>();

	// 	// 5️⃣ Build flat list for only services that belong to camp packages
	// 	var flatList = new List<ServiceStationSummaryDto>();

	// 	foreach (var assignment in assignments)
	// 	{
	// 		var type = (PackageItemType)assignment.AssignmentType;
	// 		var key = new { ReferenceId = assignment.AssignmentId, ReferenceType = type };

	// 		// Skip services not in any camp package
	// 		if (!validServiceKeys.Contains(key))
	// 			continue;

	// 		var name = await _referenceResolver.GetNameAsync(type, assignment.AssignmentId) ?? "Unknown";

	// 		var staffNames = (assignment.Subcontractor != null &&
	// 						  users.TryGetValue(assignment.Subcontractor.UserId, out var staffName))
	// 			? new List<string> { staffName }
	// 			: new List<string>();

	// 		// 🔗 Figure out which canonical package this service came from
	// 		// Match via ServicePackageItems: ServicePackageId -> ReferenceId/Type
	// 		var canonicalPackage = await _context.ServicePackageItems
	// 			.Include(spi => spi.ServicePackage)
	// 			.Where(spi => campPackages.Select(cp => cp.ServicePackageId).Contains(spi.ServicePackageId) &&
	// 						  spi.ReferenceId == assignment.AssignmentId &&
	// 						  spi.ReferenceType == type)
	// 			.Select(spi => spi.ServicePackage)
	// 			.FirstOrDefaultAsync();

	// 		var linkedCampPackage = campPackages
	// 			.FirstOrDefault(cp => cp.ServicePackageId == canonicalPackage!.Id);

	// 		flatList.Add(new ServiceStationSummaryDto
	// 		{
	// 			Id = assignment.AssignmentId,
	// 			Type = type,
	// 			Name = name,
	// 			PackageId = linkedCampPackage?.Id,
	// 			PackageName =
	// 				linkedCampPackage?.ServicePackage?.Name ??
	// 				linkedCampPackage?.DisplayName ??
	// 				"Unassigned Package",
	// 			Staff = staffNames,
	// 			PatientsServed = 0,
	// 			PendingPatients = 0,
	// 			AverageTimePerPatient = "0 min",
	// 			OutlierAlerts = 0
	// 		});
	// 	}

	// 	if (!group)
	// 		return flatList;

	// 	// 6️⃣ Optional nesting
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
	// 			groupedList.Add(station);
	// 		}
	// 	}

	// 	return groupedList;
	// }


	public async Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId, bool group = false)
	{
		// 1️⃣ Get all active camp packages (Silver, Gold, Diamond, etc.)
		var campPackages = await _context.HealthCampPackages
			.Include(p => p.ServicePackage)
			.Where(p => p.HealthCampId == healthCampId && p.IsActive)
			.ToListAsync();

		// 2️⃣ Get all package items (each item already has ServicePackageId)
		var campPackageItems = await _context.HealthCampPackageItems
			.Where(x => x.HealthCampId == healthCampId)
			.ToListAsync();

		// 3️⃣ Get all assignments for this camp
		var assignments = await _context.HealthCampServiceAssignments
			.Where(a => a.HealthCampId == healthCampId)
			.Include(a => a.Subcontractor)
			.ToListAsync();

		// 4️⃣ Build user dictionary for quick lookup
		var userIds = assignments
			.Where(a => a.Subcontractor != null)
			.Select(a => a.Subcontractor!.UserId)
			.Distinct()
			.ToList();

		var users = userIds.Any()
			? await _context.Users
				.Where(u => userIds.Contains(u.Id))
				.ToDictionaryAsync(
					u => u.Id,
					u => string.Join(" ", new[] { u.FirstName, u.MiddleName, u.LastName }
						.Where(n => !string.IsNullOrWhiteSpace(n)))
				)
			: new Dictionary<Guid, string>();

		// 5️⃣ Build flat list for all service stations
		var flatList = new List<ServiceStationSummaryDto>();

		foreach (var item in campPackageItems)
		{
			var type = item.ReferenceType;
			var name = await _referenceResolver.GetNameAsync(type, item.ReferenceId) ?? "Unknown";

			// find assigned staff
			var staffNames = assignments
				.Where(a => a.AssignmentId == item.ReferenceId && (PackageItemType)a.AssignmentType == type)
				.Select(a => a.Subcontractor?.UserId)
				.Where(uid => uid != null && users.ContainsKey(uid.Value))
				.Select(uid => users[uid!.Value])
				.Distinct()
				.ToList();

			// resolve package using ServicePackageId
			var package = campPackages.FirstOrDefault(p => p.ServicePackageId == item.ServicePackageId);

			flatList.Add(new ServiceStationSummaryDto
			{
				Id = item.ReferenceId,
				Type = type,
				Name = name,
				PackageId = package?.Id,
				PackageName =
					package?.ServicePackage?.Name ??
					package?.DisplayName ??
					"Unassigned Package",
				Staff = staffNames,
				PatientsServed = 0,
				PendingPatients = 0,
				AverageTimePerPatient = "0 min",
				OutlierAlerts = 0
			});
		}

		// 6️⃣ Return directly if grouping not required
		if (!group)
			return flatList;

		// 7️⃣ Optional hierarchical grouping
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

using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
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


	public async Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId)
	{
		var packageItems = await _context.HealthCampPackageItems
			.Where(x => x.HealthCampId == healthCampId)
			.ToListAsync();

		var assignments = await _context.HealthCampServiceAssignments
			.Where(x => x.HealthCampId == healthCampId)
			.Include(x => x.Subcontractor)
			.ToListAsync();

		var userIds = assignments
			.Select(a => a.Subcontractor.UserId)
			.Distinct()
			.ToList();

		var users = await _context.Users
			.Where(u => userIds.Contains(u.Id))
			.ToDictionaryAsync(u => u.Id, u =>
				string.Join(" ",
					new[] { u.FirstName, u.MiddleName, u.LastName }
						.Where(n => !string.IsNullOrWhiteSpace(n))
				)
			);

		var result = new List<ServiceStationSummaryDto>();

		foreach (var item in packageItems)
		{
			var name = await _referenceResolver.GetNameAsync(item.ReferenceType, item.ReferenceId);

			List<string> staffNames = new();

			if (item.ReferenceType == PackageItemType.Service)
			{
				staffNames = assignments
					.Where(a => a.ServiceId == item.ReferenceId)
					.Select(a => a.Subcontractor.UserId)
					.Where(userId => users.ContainsKey(userId))
					.Select(userId => users[userId])
					.ToList();
			}

			result.Add(new ServiceStationSummaryDto
			{
				Name = name,
				Staff = staffNames,
				PatientsServed = 0,
				PendingPatients = 0,
				AverageTimePerPatient = "0 min",
				OutlierAlerts = 0
			});
		}

		return result;
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
}

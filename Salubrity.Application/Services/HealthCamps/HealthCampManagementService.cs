using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampManagementService : IHealthCampManagementService
{
    private readonly IHealthCampManagementRepository _repo;

    public HealthCampManagementService(IHealthCampManagementRepository repo)
    {
        _repo = repo;
    }

    public Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId, bool group)
        => _repo.GetServiceStationsAsync(healthCampId, group);

    public Task<List<CampPatientSummaryDto>> GetCampPatientsAsync(Guid healthCampId)
        => _repo.GetCampPatientsAsync(healthCampId);

    public Task<List<CampDailyActivityDto>> GetActivitySummaryAsync(Guid healthCampId)
        => _repo.GetActivitySummaryAsync(healthCampId);

    public Task<List<CampBillingSummaryDto>> GetBillingSummaryAsync(Guid healthCampId)
        => _repo.GetBillingSummaryAsync(healthCampId);

    public async Task<OrganizationCampDetailsDto> GetOrganizationCampDetailsAsync(Guid healthCampId)
    {
        var camp = await _repo.GetWithDetailsAsync(healthCampId)
            ?? throw new NotFoundException("Health camp not found");

        var organization = camp.Organization;
        var venue = camp.Location ?? "�";

        var insurer = organization.InsuranceProviders
            .Select(x => x.InsuranceProvider.Name)
            .FirstOrDefault() ?? "�";

        var packageName = organization.Packages
            .Select(p => p.ServicePackage.Name)
            .FirstOrDefault() ?? "�";

        var packageCost = organization.Packages
            .Select(p => p.ServicePackage.Price)
            .FirstOrDefault();

        var costDisplay = packageCost.HasValue
            ? $"Ksh {packageCost.Value:N0} Per Person"
            : "�";

        return new OrganizationCampDetailsDto
        {
            CampName = camp.Name,
            StartDateTime = camp.StartDate.Add(camp.StartTime ?? TimeSpan.Zero),

            ClientName = organization.BusinessName,
            Venue = venue,
            ExpectedPatients = 68, // TODO: Replace with actual source if available
            SubcontractorCount = camp.ServiceAssignments.Count,
            SubcontractorsReady = camp.ServiceAssignments.Count > 0,

            Insurer = insurer,
            PackageName = packageName,
            PackageCost = costDisplay
        };
    }


}

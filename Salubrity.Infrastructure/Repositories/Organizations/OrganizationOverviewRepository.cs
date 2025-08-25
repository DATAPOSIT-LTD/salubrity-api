using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Organizations;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Infrastructure.Persistence;
using System.Security.Claims;

namespace Salubrity.Infrastructure.Repositories.Organizations
{
    public class OrganizationOverviewRepository : IOrganizationOverviewRepository
    {
        private readonly AppDbContext _context;

        public OrganizationOverviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrganizationOverviewDto> GetOrganizationOverviewAsync()
        {
            // Onboarded Organizations
            var onboardedOrganizations = await _context.Set<Organization>().CountAsync();

            // Total patients onboarded (patients tied to organizations)
            var totalPatientsOnboarded = await _context.Set<HealthCampParticipant>()
                .Where(p => p.PatientId != null && p.HealthCamp.OrganizationId != null)
                .Select(p => p.PatientId)
                .Distinct()
                .CountAsync();

            // Total Patients Checked (patients who have completed a camp)
            var totalPatientsChecked = await _context.Set<HealthCampParticipant>()
                .Where(p => p.PatientId != null && p.HealthCamp.HealthCampStatus != null && p.HealthCamp.HealthCampStatus.Name == "Completed")
                .Select(p => p.PatientId)
                .Distinct()
                .CountAsync();

            // Total paid claims (sum of paid claims)
            //var totalPaidClaims = await _context.Set<HealthCamp>()
            //    .Where(c => c.Status == "Paid")
            //    .SumAsync(c => (decimal?)c.AmountPaid ?? 0);

            return new OrganizationOverviewDto
            {
                OnboardedOrganizations = onboardedOrganizations,
                TotalPatientsOnboarded = totalPatientsOnboarded,
                TotalPatientsChecked = totalPatientsChecked,
                //TotalPaidClaims = totalPaidClaims
            };
        }
    }
}

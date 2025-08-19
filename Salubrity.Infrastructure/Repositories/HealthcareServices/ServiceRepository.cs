#nullable enable
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthcareServices
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ServiceRepository> _logger;

        public ServiceRepository(AppDbContext db, ILogger<ServiceRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ========== BASIC CRUD OPERATIONS ==========

        public async Task AddAsync(Service entity, CancellationToken ct = default)
        {
            await _db.Services.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            _logger.LogDebug("Added service {ServiceId}: {ServiceName}", entity.Id, entity.Name);
        }

        public async Task UpdateAsync(Service entity, CancellationToken ct = default)
        {
            _db.Services.Update(entity);
            await _db.SaveChangesAsync(ct);
            _logger.LogDebug("Updated service {ServiceId}: {ServiceName}", entity.Id, entity.Name);
        }

        public async Task DeleteAsync(Service entity, CancellationToken ct = default)
        {
            // Soft delete by setting IsActive = false (recommended for healthcare data)
            entity.IsActive = false;
            _db.Services.Update(entity);
            await _db.SaveChangesAsync(ct);
            _logger.LogDebug("Soft deleted service {ServiceId}: {ServiceName}", entity.Id, entity.Name);
        }

        public async Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Services
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(ct);
        }

        public async Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Services
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, ct);
        }

        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Services.AnyAsync(s => s.Id == id && s.IsActive, ct);
        }

        // ========== HIERARCHY-AWARE OPERATIONS ==========

        public async Task<IReadOnlyList<Service>> GetAllWithHierarchyAsync(bool includeInactive = false, CancellationToken ct = default)
        {
            var query = _db.Services
                .AsNoTracking()
                .Include(s => s.Categories.Where(c => includeInactive || c.IsActive))
                    .ThenInclude(c => c.Subcategories.Where(sc => includeInactive || sc.IsActive))
                .Include(s => s.Industry)
                .Include(s => s.IntakeForm)
                    .ThenInclude(f => f.Sections!)
                        .ThenInclude(sec => sec.Fields); // Load fields within sections

            return await query.OrderBy(s => s.Name).ToListAsync(ct);
        }


        public async Task<Service?> GetByIdWithHierarchyAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Services
                .Include(s => s.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Subcategories.Where(sc => sc.IsActive))
                .Include(s => s.Industry)
                .Include(s => s.IntakeForm)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, ct);
        }

        // ========== VALIDATION & CONSTRAINTS ==========

        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _db.Services.Where(s => s.Name.ToLower() == name.ToLower() && s.IsActive);

            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return !await query.AnyAsync(ct);
        }

        public async Task<bool> ValidateReferencesAsync(Guid? industryId, Guid? intakeFormId, CancellationToken ct = default)
        {
            if (industryId.HasValue)
            {
                var industryExists = await _db.Industries.AnyAsync(i => i.Id == industryId.Value && i.IsActive, ct);
                if (!industryExists)
                {
                    _logger.LogWarning("Invalid industry reference: {IndustryId}", industryId);
                    return false;
                }
            }

            if (intakeFormId.HasValue)
            {
                var intakeFormExists = await _db.IntakeForms.AnyAsync(f => f.Id == intakeFormId.Value && !f.IsDeleted, ct);
                if (!intakeFormExists)
                {
                    _logger.LogWarning("Invalid intake form reference: {IntakeFormId}", intakeFormId);
                    return false;
                }
            }

            return true;
        }

        // ========== QUERY OPERATIONS ==========

        public async Task<IReadOnlyList<Service>> GetByIndustryAsync(Guid industryId, CancellationToken ct = default)
        {
            return await _db.Services
                .AsNoTracking()
                .Include(s => s.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Subcategories.Where(sc => sc.IsActive))
                .Where(s => s.IndustryId == industryId && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Service>> GetServicesWithIntakeFormsAsync(CancellationToken ct = default)
        {
            return await _db.Services
                .AsNoTracking()
                .Include(s => s.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Subcategories.Where(sc => sc.IsActive))
                .Include(s => s.IntakeForm)
                .Where(s => s.IntakeFormId.HasValue && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Service>> SearchAsync(string searchTerm, CancellationToken ct = default)
        {
            var normalizedSearch = searchTerm.ToLower().Trim();

            return await _db.Services
                .AsNoTracking()
                .Include(s => s.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Subcategories.Where(sc => sc.IsActive))
                .Where(s => s.IsActive && (
                    s.Name.ToLower().Contains(normalizedSearch) ||
                    (s.Description != null && s.Description.ToLower().Contains(normalizedSearch))
                ))
                .OrderBy(s => s.Name)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Service>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken ct = default)
        {
            return await _db.Services
                .AsNoTracking()
                .Include(s => s.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Subcategories.Where(sc => sc.IsActive))
                .Where(s => s.IsActive &&
                           s.PricePerPerson.HasValue &&
                           s.PricePerPerson.Value >= minPrice &&
                           s.PricePerPerson.Value <= maxPrice)
                .OrderBy(s => s.PricePerPerson)
                .ToListAsync(ct);
        }

        // ========== BUSINESS LOGIC QUERIES ==========

        public async Task<bool> HasActiveBookingsAsync(Guid serviceId, CancellationToken ct = default)
        {
            // This would integrate with your booking system
            // For now, checking if any appointments reference this service
            // Adjust the table/entity names based on your actual booking schema
            return await _db.Set<object>() // Replace with actual Booking/Appointment entity
                .FromSqlRaw(@"
                    SELECT 1 FROM Appointments a 
                    INNER JOIN ServiceSubcategories sc ON a.ServiceSubcategoryId = sc.Id
                    INNER JOIN ServiceCategories c ON sc.ServiceCategoryId = c.Id
                    WHERE c.ServiceId = {0} AND a.Status = 'Active'
                    UNION
                    SELECT 1 FROM Appointments a
                    INNER JOIN ServiceCategories c ON a.ServiceCategoryId = c.Id  
                    WHERE c.ServiceId = {0} AND a.Status = 'Active'", serviceId)
                .AnyAsync(ct);
        }

        public async Task<IReadOnlyList<Service>> GetMostPopularServicesAsync(int limit = 10, CancellationToken ct = default)
        {
            // This would join with booking/appointment data to get popularity
            // For now, returning services ordered by category count as a proxy
            return await _db.Services
                .AsNoTracking()
                .Include(s => s.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Subcategories.Where(sc => sc.IsActive))
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.Categories.Count)
                .Take(limit)
                .ToListAsync(ct);
        }

        // ========== TRANSACTION MANAGEMENT ==========

        public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            var transaction = await _db.Database.BeginTransactionAsync(ct);
            return transaction.GetDbTransaction();
        }

        // ========== BATCH OPERATIONS ==========

        public async Task AddRangeAsync(IEnumerable<Service> services, CancellationToken ct = default)
        {
            await _db.Services.AddRangeAsync(services, ct);
            await _db.SaveChangesAsync(ct);
            _logger.LogDebug("Added {Count} services in batch", services.Count());
        }

        public async Task UpdateRangeAsync(IEnumerable<Service> services, CancellationToken ct = default)
        {
            _db.Services.UpdateRange(services);
            await _db.SaveChangesAsync(ct);
            _logger.LogDebug("Updated {Count} services in batch", services.Count());
        }

        public async Task SoftDeleteRangeAsync(IEnumerable<Guid> serviceIds, CancellationToken ct = default)
        {
            await _db.Services
                .Where(s => serviceIds.Contains(s.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false), ct);

            _logger.LogDebug("Soft deleted {Count} services", serviceIds.Count());
        }

        // ========== ADVANCED HIERARCHY OPERATIONS ==========

        public async Task<IReadOnlyList<(Service Service, int CategoryCount, int SubcategoryCount)>> GetServicesWithHierarchyStatsAsync(CancellationToken ct = default)
        {
            return await _db.Services
                .AsNoTracking()
                .Include(s => s.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Subcategories.Where(sc => sc.IsActive))
                .Where(s => s.IsActive)
                .Select(s => new ValueTuple<Service, int, int>(
                    s,
                    s.Categories.Count(c => c.IsActive),
                    s.Categories.Sum(c => c.Subcategories.Count(sc => sc.IsActive))
                ))
                .ToListAsync(ct);
        }

        public async Task ReplaceCategoryTreeAsync(Guid serviceId, IEnumerable<ServiceCategory> newCategories, CancellationToken ct = default)
        {
            var trackedService = await _db.Services
                .Include(s => s.Categories)
                    .ThenInclude(c => c.Subcategories)
                .FirstOrDefaultAsync(s => s.Id == serviceId, ct);

            if (trackedService is null)
                throw new InvalidOperationException($"Service with ID {serviceId} not found when replacing category tree.");

            _logger.LogDebug("Replacing category tree for service {ServiceId}", serviceId);

            var currentCats = trackedService.Categories.ToDictionary(c => c.Id, c => c);
            var newCatList = (newCategories ?? Enumerable.Empty<ServiceCategory>()).ToList();
            var newCatsById = newCatList
                .Where(c => c.Id != Guid.Empty)
                .GroupBy(c => c.Id)
                .ToDictionary(g => g.Key, g => g.First());

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // 1) DELETE categories missing in new set
                var categoriesToDelete = currentCats.Values.Where(c => !newCatsById.ContainsKey(c.Id)).ToList();
                foreach (var categoryToDelete in categoriesToDelete)
                {
                    _db.ServiceSubcategories.RemoveRange(categoryToDelete.Subcategories);
                    _db.ServiceCategories.Remove(categoryToDelete);
                }

                // 2) ADD or UPDATE categories
                foreach (var incomingCat in newCatList)
                {
                    incomingCat.ServiceId = trackedService.Id;

                    if (incomingCat.Id != Guid.Empty && currentCats.TryGetValue(incomingCat.Id, out var existingCat))
                    {
                        // UPDATE existing category
                        existingCat.Name = incomingCat.Name;
                        existingCat.Description = incomingCat.Description;
                        existingCat.Price = incomingCat.Price;
                        existingCat.DurationMinutes = incomingCat.DurationMinutes;
                        existingCat.IsActive = incomingCat.IsActive;

                        // Handle subcategories with replace semantics
                        await ReplaceSubcategoriesForCategory(existingCat, incomingCat.Subcategories ?? [], ct);
                    }
                    else
                    {
                        // CREATE new category
                        if (incomingCat.Id == Guid.Empty)
                            incomingCat.Id = Guid.NewGuid();

                        // Set up subcategories
                        foreach (var subcategory in incomingCat.Subcategories ?? Enumerable.Empty<ServiceSubcategory>())
                        {
                            subcategory.ServiceCategoryId = incomingCat.Id;
                            if (subcategory.Id == Guid.Empty)
                                subcategory.Id = Guid.NewGuid();
                        }

                        _db.ServiceCategories.Add(incomingCat);
                    }
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogDebug("Successfully replaced category tree for service {ServiceId}", serviceId);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "Failed to replace category tree for service {ServiceId}", serviceId);
                throw;
            }
        }

        // ========== LEGACY COMPATIBILITY ==========

        [Obsolete("Use IsNameUniqueAsync instead", false)]
        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        {
            return !await IsNameUniqueAsync(name, excludeId, ct);
        }

        [Obsolete("Use GetAllWithHierarchyAsync instead", false)]
        public async Task<IReadOnlyList<Service>> GetAllWithTreeAsync(CancellationToken ct = default)
        {
            return await GetAllWithHierarchyAsync(includeInactive: false, ct);
        }

        [Obsolete("Use GetByIdWithHierarchyAsync instead", false)]
        public async Task<Service?> GetByIdWithTreeAsync(Guid id, CancellationToken ct = default)
        {
            return await GetByIdWithHierarchyAsync(id, ct);
        }

        [Obsolete("Use ReplaceCategoryTreeAsync instead", false)]
        public async Task ReplaceCategoriesTreeAsync(Service service, IEnumerable<ServiceCategory> newCategories, CancellationToken ct = default)
        {
            await ReplaceCategoryTreeAsync(service.Id, newCategories, ct);
        }

        // ========== PRIVATE HELPER METHODS ==========

        private async Task ReplaceSubcategoriesForCategory(ServiceCategory existingCategory, ICollection<ServiceSubcategory> newSubcategories, CancellationToken ct = default)
        {
            var existingSubs = existingCategory.Subcategories.ToDictionary(s => s.Id, s => s);
            var newSubsList = newSubcategories.ToList();
            var newSubsById = newSubsList
                .Where(s => s.Id != Guid.Empty)
                .GroupBy(s => s.Id)
                .ToDictionary(g => g.Key, g => g.First());

            // Delete subcategories not in the new list
            var subcategoriesToDelete = existingSubs.Values.Where(s => !newSubsById.ContainsKey(s.Id)).ToList();
            foreach (var subcategoryToDelete in subcategoriesToDelete)
            {
                _db.ServiceSubcategories.Remove(subcategoryToDelete);
            }

            // Add or update subcategories
            foreach (var incomingSub in newSubsList)
            {
                incomingSub.ServiceCategoryId = existingCategory.Id;

                if (incomingSub.Id != Guid.Empty && existingSubs.TryGetValue(incomingSub.Id, out var existingSub))
                {
                    // UPDATE existing subcategory
                    existingSub.Name = incomingSub.Name;
                    existingSub.Description = incomingSub.Description;
                    existingSub.Price = incomingSub.Price;
                    existingSub.DurationMinutes = incomingSub.DurationMinutes;
                    existingSub.IsActive = incomingSub.IsActive;
                }
                else
                {
                    // CREATE new subcategory
                    if (incomingSub.Id == Guid.Empty)
                        incomingSub.Id = Guid.NewGuid();

                    _db.ServiceSubcategories.Add(incomingSub);
                }
            }
        }
    }
}
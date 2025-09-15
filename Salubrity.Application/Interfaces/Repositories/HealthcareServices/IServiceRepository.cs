#nullable enable
using System.Data;
using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Interfaces.Repositories.HealthcareServices
{
	/// <summary>
	/// Repository interface for Service entity with full hierarchy support
	/// </summary>
	public interface IServiceRepository
	{
		// ========== BASIC CRUD OPERATIONS ==========

		/// <summary>
		/// Retrieves all services (without hierarchy)
		/// </summary>
		/// <param name="ct">Cancellation token</param>
		/// <returns>List of services</returns>
		Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken ct = default);

		/// <summary>
		/// Retrieves a service by ID (without hierarchy)
		/// </summary>
		/// <param name="id">Service identifier</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Service entity or null if not found</returns>
		Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default);

		/// <summary>
		/// Adds a new service to the repository
		/// </summary>
		/// <param name="entity">Service entity to add</param>
		/// <param name="ct">Cancellation token</param>
		Task AddAsync(Service entity, CancellationToken ct = default);

		/// <summary>
		/// Updates an existing service
		/// </summary>
		/// <param name="entity">Service entity to update</param>
		/// <param name="ct">Cancellation token</param>
		Task UpdateAsync(Service entity, CancellationToken ct = default);

		/// <summary>
		/// Deletes a service (soft delete recommended)
		/// </summary>
		/// <param name="entity">Service entity to delete</param>
		/// <param name="ct">Cancellation token</param>
		Task DeleteAsync(Service entity, CancellationToken ct = default);

		// ========== HIERARCHY-AWARE OPERATIONS ==========

		/// <summary>
		/// Retrieves all services with full hierarchy (Categories -> Subcategories)
		/// </summary>
		/// <param name="includeInactive">Whether to include inactive services</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Services with full hierarchy loaded</returns>
		Task<IReadOnlyList<Service>> GetAllWithHierarchyAsync(bool includeInactive = false, CancellationToken ct = default);

		/// <summary>
		/// Retrieves a service by ID with full hierarchy loaded
		/// </summary>
		/// <param name="id">Service identifier</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Service with full hierarchy or null if not found</returns>
		Task<Service?> GetByIdWithHierarchyAsync(Guid id, CancellationToken ct = default);

		// ========== VALIDATION & CONSTRAINTS ==========

		/// <summary>
		/// Checks if a service name is unique
		/// </summary>
		/// <param name="name">Service name to check</param>
		/// <param name="excludeId">Service ID to exclude from check (for updates)</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>True if name is unique</returns>
		Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

		/// <summary>
		/// Checks if a service exists by ID
		/// </summary>
		/// <param name="id">Service identifier</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>True if service exists</returns>
		Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

		/// <summary>
		/// Validates that referenced entities (Industry, IntakeForm) exist
		/// </summary>
		/// <param name="industryId">Industry identifier</param>
		/// <param name="intakeFormId">Intake form identifier</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>True if all references are valid</returns>
		Task<bool> ValidateReferencesAsync(Guid? industryId, Guid? intakeFormId, CancellationToken ct = default);

		// ========== QUERY OPERATIONS ==========

		/// <summary>
		/// Retrieves services filtered by industry
		/// </summary>
		/// <param name="industryId">Industry identifier</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Services in the specified industry</returns>
		Task<IReadOnlyList<Service>> GetByIndustryAsync(Guid industryId, CancellationToken ct = default);

		/// <summary>
		/// Retrieves services that have intake forms assigned
		/// </summary>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Services with intake forms</returns>
		Task<IReadOnlyList<Service>> GetServicesWithIntakeFormsAsync(CancellationToken ct = default);

		/// <summary>
		/// Searches services by name or description (case-insensitive)
		/// </summary>
		/// <param name="searchTerm">Search term</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Matching services</returns>
		Task<IReadOnlyList<Service>> SearchAsync(string searchTerm, CancellationToken ct = default);

		/// <summary>
		/// Retrieves services within a price range
		/// </summary>
		/// <param name="minPrice">Minimum price (inclusive)</param>
		/// <param name="maxPrice">Maximum price (inclusive)</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Services within price range</returns>
		Task<IReadOnlyList<Service>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken ct = default);

		// ========== BUSINESS LOGIC QUERIES ==========

		/// <summary>
		/// Checks if a service has any active bookings/appointments
		/// </summary>
		/// <param name="serviceId">Service identifier</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>True if service has active bookings</returns>
		Task<bool> HasActiveBookingsAsync(Guid serviceId, CancellationToken ct = default);

		/// <summary>
		/// Gets services ordered by popularity (booking count)
		/// </summary>
		/// <param name="limit">Maximum number of services to return</param>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Most popular services</returns>
		Task<IReadOnlyList<Service>> GetMostPopularServicesAsync(int limit = 10, CancellationToken ct = default);

		// ========== TRANSACTION MANAGEMENT ==========

		/// <summary>
		/// Begins a database transaction for complex operations
		/// </summary>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Database transaction</returns>
		Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default);

		// ========== BATCH OPERATIONS ==========

		/// <summary>
		/// Adds multiple services in a single batch operation
		/// </summary>
		/// <param name="services">Services to add</param>
		/// <param name="ct">Cancellation token</param>
		Task AddRangeAsync(IEnumerable<Service> services, CancellationToken ct = default);

		/// <summary>
		/// Updates multiple services in a single batch operation
		/// </summary>
		/// <param name="services">Services to update</param>
		/// <param name="ct">Cancellation token</param>
		Task UpdateRangeAsync(IEnumerable<Service> services, CancellationToken ct = default);

		/// <summary>
		/// Soft deletes multiple services in a single batch operation
		/// </summary>
		/// <param name="serviceIds">Service identifiers to delete</param>
		/// <param name="ct">Cancellation token</param>
		Task SoftDeleteRangeAsync(IEnumerable<Guid> serviceIds, CancellationToken ct = default);

		// ========== ADVANCED HIERARCHY OPERATIONS ==========

		/// <summary>
		/// Gets services with their category count and subcategory count
		/// </summary>
		/// <param name="ct">Cancellation token</param>
		/// <returns>Services with hierarchy statistics</returns>
		Task<IReadOnlyList<(Service Service, int CategoryCount, int SubcategoryCount)>> GetServicesWithHierarchyStatsAsync(CancellationToken ct = default);

		/// <summary>
		/// Replaces the entire category tree for a service (handles create/update/delete)
		/// </summary>
		/// <param name="serviceId">Service identifier</param>
		/// <param name="newCategories">New category structure</param>
		/// <param name="ct">Cancellation token</param>
		/// <remarks>
		/// This method implements the replace semantics for nested updates:
		/// - Categories with ID: Update existing
		/// - Categories without ID: Create new
		/// - Categories not in list: Delete existing
		/// </remarks>
		Task ReplaceCategoryTreeAsync(Guid serviceId, IEnumerable<ServiceCategory> newCategories, CancellationToken ct = default);

		// ========== LEGACY COMPATIBILITY ==========

		/// <summary>
		/// Legacy method name - use IsNameUniqueAsync instead
		/// </summary>
		[Obsolete("Use IsNameUniqueAsync instead", false)]
		Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

		/// <summary>
		/// Legacy method name - use GetAllWithHierarchyAsync instead
		/// </summary>
		[Obsolete("Use GetAllWithHierarchyAsync instead", false)]
		Task<IReadOnlyList<Service>> GetAllWithTreeAsync(CancellationToken ct = default);

		/// <summary>
		/// Legacy method name - use GetByIdWithHierarchyAsync instead
		/// </summary>
		[Obsolete("Use GetByIdWithHierarchyAsync instead", false)]
		Task<Service?> GetByIdWithTreeAsync(Guid id, CancellationToken ct = default);

		/// <summary>
		/// Legacy method name - use ReplaceCategoryTreeAsync instead
		/// </summary>
		[Obsolete("Use ReplaceCategoryTreeAsync instead", false)]
		Task ReplaceCategoriesTreeAsync(Service service, IEnumerable<ServiceCategory> newCategories, CancellationToken ct = default);
		Task<Service?> GetByIdWithCategoriesAsync(Guid id);
	}
}
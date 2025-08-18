using Salubrity.Application.DTOs.HealthcareServices;

namespace Salubrity.Application.Interfaces.Services.HealthcareServices;

/// <summary>
/// Service interface for managing healthcare services with full hierarchy support
/// </summary>
public interface IServiceService
{
    // ========== BASIC CRUD OPERATIONS ==========

    /// <summary>
    /// Retrieves all active services with their categories and subcategories
    /// </summary>
    /// <returns>List of services with full hierarchy</returns>
    Task<List<ServiceResponseDto>> GetAllAsync();

    /// <summary>
    /// Retrieves all services (including inactive) with their categories and subcategories
    /// </summary>
    /// <returns>List of all services with full hierarchy</returns>
    Task<List<ServiceResponseDto>> GetAllIncludingInactiveAsync();

    /// <summary>
    /// Retrieves a service by ID with its full hierarchy
    /// </summary>
    /// <param name="id">Service identifier</param>
    /// <returns>Service with categories and subcategories</returns>
    /// <exception cref="NotFoundException">When service is not found</exception>
    Task<ServiceResponseDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new service with nested categories and subcategories
    /// </summary>
    /// <param name="input">Service creation data with nested hierarchy</param>
    /// <returns>Created service with generated IDs</returns>
    Task<ServiceResponseDto> CreateAsync(CreateServiceDto input);

    /// <summary>
    /// Updates a service using replace semantics for categories/subcategories
    /// </summary>
    /// <param name="id">Service identifier</param>
    /// <param name="input">Service update data with nested hierarchy</param>
    /// <returns>Updated service with full hierarchy</returns>
    /// <exception cref="NotFoundException">When service is not found</exception>
    Task<ServiceResponseDto> UpdateAsync(Guid id, UpdateServiceDto input);

    /// <summary>
    /// Soft deletes a service and all its categories/subcategories
    /// </summary>
    /// <param name="id">Service identifier</param>
    /// <exception cref="NotFoundException">When service is not found</exception>
    Task DeleteAsync(Guid id);

    // ========== QUERY OPERATIONS ==========

    /// <summary>
    /// Retrieves services filtered by industry
    /// </summary>
    /// <param name="industryId">Industry identifier</param>
    /// <returns>Services belonging to the specified industry</returns>
    Task<List<ServiceResponseDto>> GetByIndustryAsync(Guid industryId);

    /// <summary>
    /// Retrieves services that have an intake form assigned
    /// </summary>
    /// <returns>Services with intake forms</returns>
    Task<List<ServiceResponseDto>> GetServicesWithIntakeFormsAsync();

    /// <summary>
    /// Searches services by name or description (case-insensitive)
    /// </summary>
    /// <param name="searchTerm">Search term to match against name or description</param>
    /// <returns>Matching services</returns>
    Task<List<ServiceResponseDto>> SearchAsync(string searchTerm);

    /// <summary>
    /// Retrieves services within a price range (considers base service price)
    /// </summary>
    /// <param name="minPrice">Minimum price (inclusive)</param>
    /// <param name="maxPrice">Maximum price (inclusive)</param>
    /// <returns>Services within the price range</returns>
    Task<List<ServiceResponseDto>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);

    // ========== ACTIVATION OPERATIONS ==========

    /// <summary>
    /// Activates a service and optionally its categories/subcategories
    /// </summary>
    /// <param name="id">Service identifier</param>
    /// <param name="activateChildren">Whether to activate all child categories/subcategories</param>
    /// <exception cref="NotFoundException">When service is not found</exception>
    Task ActivateAsync(Guid id, bool activateChildren = false);

    /// <summary>
    /// Deactivates a service and optionally its categories/subcategories
    /// </summary>
    /// <param name="id">Service identifier</param>
    /// <param name="deactivateChildren">Whether to deactivate all child categories/subcategories</param>
    /// <exception cref="NotFoundException">When service is not found</exception>
    Task DeactivateAsync(Guid id, bool deactivateChildren = true);

    // ========== VALIDATION OPERATIONS ==========

    /// <summary>
    /// Checks if a service name is unique within the system
    /// </summary>
    /// <param name="name">Service name to check</param>
    /// <param name="excludeServiceId">Service ID to exclude from check (for updates)</param>
    /// <returns>True if name is available</returns>
    Task<bool> IsServiceNameUniqueAsync(string name, Guid? excludeServiceId = null);

    /// <summary>
    /// Validates that all referenced entities (Industry, IntakeForm) exist
    /// </summary>
    /// <param name="industryId">Industry identifier</param>
    /// <param name="intakeFormId">Intake form identifier</param>
    /// <returns>True if all references are valid</returns>
    Task<bool> ValidateReferencesAsync(Guid? industryId, Guid? intakeFormId);

    // ========== CATEGORY OPERATIONS ==========

    /// <summary>
    /// Adds a new category to an existing service
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="categoryDto">Category creation data</param>
    /// <returns>Updated service with new category</returns>
    /// <exception cref="NotFoundException">When service is not found</exception>
    Task<ServiceResponseDto> AddCategoryAsync(Guid serviceId, CreateServiceCategoryDto categoryDto);

    /// <summary>
    /// Updates a specific category within a service
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="categoryDto">Category update data</param>
    /// <returns>Updated service</returns>
    /// <exception cref="NotFoundException">When service or category is not found</exception>
    Task<ServiceResponseDto> UpdateCategoryAsync(Guid serviceId, Guid categoryId, UpdateServiceCategoryDto categoryDto);

    /// <summary>
    /// Removes a category and all its subcategories from a service
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="categoryId">Category identifier</param>
    /// <returns>Updated service without the category</returns>
    /// <exception cref="NotFoundException">When service or category is not found</exception>
    Task<ServiceResponseDto> RemoveCategoryAsync(Guid serviceId, Guid categoryId);

    // ========== BUSINESS LOGIC OPERATIONS ==========

    /// <summary>
    /// Calculates the effective price for a service (falls back through hierarchy)
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="categoryId">Optional category identifier</param>
    /// <param name="subcategoryId">Optional subcategory identifier</param>
    /// <returns>Effective price at the specified level</returns>
    Task<decimal?> GetEffectivePriceAsync(Guid serviceId, Guid? categoryId = null, Guid? subcategoryId = null);

    /// <summary>
    /// Gets the total duration for a service including all subcategories
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <returns>Total duration in minutes</returns>
    Task<int> GetTotalDurationAsync(Guid serviceId);

    /// <summary>
    /// Checks if a service has any active bookings/appointments
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <returns>True if service has active bookings</returns>
    Task<bool> HasActiveBookingsAsync(Guid serviceId);

    Task AssignFormAsync(AssignFormToServiceDto dto);

}
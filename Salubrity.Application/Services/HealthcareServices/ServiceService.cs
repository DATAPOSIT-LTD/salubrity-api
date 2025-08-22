using AutoMapper;
using Microsoft.Extensions.Logging;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Shared.Exceptions;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.DTOs.Forms;

namespace Salubrity.Application.Services.HealthcareServices;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _serviceRepo;
    private readonly IServiceCategoryRepository _categoryRepo;
    private readonly IServiceSubcategoryRepository _subcategoryRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<ServiceService> _logger;
    private readonly IIntakeFormRepository _formRepo;

    public ServiceService(
        IServiceRepository serviceRepo,
        IServiceCategoryRepository categoryRepo,
        IServiceSubcategoryRepository subcategoryRepo,
        IMapper mapper,
        IIntakeFormRepository intakeForm,
        ILogger<ServiceService> logger)
    {
        _serviceRepo = serviceRepo;
        _categoryRepo = categoryRepo;
        _subcategoryRepo = subcategoryRepo;
        _mapper = mapper;
        _logger = logger;
        _formRepo = intakeForm;
    }

    // ========== BASIC CRUD OPERATIONS ==========

    public async Task<List<ServiceResponseDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all active services with hierarchy");
        var services = await _serviceRepo.GetAllWithHierarchyAsync(includeInactive: false);
        return _mapper.Map<List<ServiceResponseDto>>(services);
    }

    public async Task<List<ServiceResponseDto>> GetAllIncludingInactiveAsync()
    {
        _logger.LogInformation("Retrieving all services (including inactive) with hierarchy");
        var services = await _serviceRepo.GetAllWithHierarchyAsync(includeInactive: true);
        return _mapper.Map<List<ServiceResponseDto>>(services);
    }

    public async Task<ServiceResponseDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving service {ServiceId} with hierarchy", id);

        var service = await _serviceRepo.GetByIdWithHierarchyAsync(id)
            ?? throw new NotFoundException($"Service with ID {id} not found");

        // Map base service
        var dto = _mapper.Map<ServiceResponseDto>(service);

        // Manually map the attached IntakeForm if present
        if (service.IntakeForm is not null)
        {
            dto.IntakeForm = _mapper.Map<FormResponseDto>(service.IntakeForm);
        }

        return dto;
    }


    public async Task<ServiceResponseDto> CreateAsync(CreateServiceDto input)
    {
        _logger.LogInformation("Creating new service: {ServiceName}", input.Name);

        // Validate unique name
        if (!await IsServiceNameUniqueAsync(input.Name))
        {
            throw new ValidationException(["Service with the same name already exists"]);
        }

        // Validate references if provided
        if (!await ValidateReferencesAsync(input.IndustryId, input.IntakeFormId))
        {
            throw new ValidationException(["Invalid Industry or IntakeForm reference"]);
        }

        using var transaction = await _serviceRepo.BeginTransactionAsync();
        try
        {
            // Create main service
            var service = _mapper.Map<Service>(input);
            service.Id = Guid.NewGuid();
            await _serviceRepo.AddAsync(service);

            // Create categories and subcategories
            foreach (var categoryDto in input.Categories)
            {
                var category = _mapper.Map<ServiceCategory>(categoryDto);
                category.Id = Guid.NewGuid();
                category.ServiceId = service.Id;
                await _categoryRepo.AddAsync(category);

                // Create subcategories
                foreach (var subcategoryDto in categoryDto.Subcategories)
                {
                    var subcategory = _mapper.Map<ServiceSubcategory>(subcategoryDto);
                    subcategory.Id = Guid.NewGuid();
                    subcategory.ServiceCategoryId = category.Id;
                    await _subcategoryRepo.AddAsync(subcategory);
                }
            }

            transaction.Commit();
            _logger.LogInformation("Successfully created service {ServiceId}", service.Id);

            // Return the created service with full hierarchy
            return await GetByIdAsync(service.Id);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<ServiceResponseDto> UpdateAsync(Guid id, UpdateServiceDto input)
    {
        _logger.LogInformation("Updating service {ServiceId}", id);

        var service = await _serviceRepo.GetByIdWithHierarchyAsync(id)
            ?? throw new NotFoundException($"Service with ID {id} not found");

        // Validate unique name (excluding current service)
        if (!await IsServiceNameUniqueAsync(input.Name, id))
        {
            throw new ValidationException(["Another service with the same name already exists"]);
        }

        // Validate references if provided
        if (!await ValidateReferencesAsync(input.IndustryId, input.IntakeFormId))
        {
            throw new ValidationException(["Invalid Industry or IntakeForm reference"]);
        }

        using var transaction = await _serviceRepo.BeginTransactionAsync();
        try
        {
            // Update main service properties
            _mapper.Map(input, service);
            await _serviceRepo.UpdateAsync(service);

            // Handle categories with replace semantics
            await UpdateCategoriesWithReplaceSemantics(service, input.Categories);

            transaction.Commit();
            _logger.LogInformation("Successfully updated service {ServiceId}", id);

            return await GetByIdAsync(id);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting service {ServiceId}", id);

        var service = await _serviceRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service with ID {id} not found");

        // Check for active bookings before deletion
        if (await HasActiveBookingsAsync(id))
        {
            throw new ValidationException(["Cannot delete service with active bookings"]);
        }

        await _serviceRepo.DeleteAsync(service);
        _logger.LogInformation("Successfully deleted service {ServiceId}", id);
    }

    // ========== QUERY OPERATIONS ==========

    public async Task<List<ServiceResponseDto>> GetByIndustryAsync(Guid industryId)
    {
        var services = await _serviceRepo.GetByIndustryAsync(industryId);
        return _mapper.Map<List<ServiceResponseDto>>(services);
    }

    public async Task<List<ServiceResponseDto>> GetServicesWithIntakeFormsAsync()
    {
        var services = await _serviceRepo.GetServicesWithIntakeFormsAsync();
        return _mapper.Map<List<ServiceResponseDto>>(services);
    }

    public async Task<List<ServiceResponseDto>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return [];

        var services = await _serviceRepo.SearchAsync(searchTerm.Trim());
        return _mapper.Map<List<ServiceResponseDto>>(services);
    }

    public async Task<List<ServiceResponseDto>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        if (minPrice < 0 || maxPrice < minPrice)
            throw new ValidationException(["Invalid price range"]);

        var services = await _serviceRepo.GetByPriceRangeAsync(minPrice, maxPrice);
        return _mapper.Map<List<ServiceResponseDto>>(services);
    }

    // ========== ACTIVATION OPERATIONS ==========

    public async Task ActivateAsync(Guid id, bool activateChildren = false)
    {
        var service = await _serviceRepo.GetByIdWithHierarchyAsync(id)
            ?? throw new NotFoundException($"Service with ID {id} not found");

        service.IsActive = true;

        if (activateChildren)
        {
            foreach (var category in service.Categories)
            {
                category.IsActive = true;
                foreach (var subcategory in category.Subcategories)
                {
                    subcategory.IsActive = true;
                }
            }
        }

        await _serviceRepo.UpdateAsync(service);
        _logger.LogInformation("Activated service {ServiceId}, children: {ActivateChildren}", id, activateChildren);
    }

    public async Task DeactivateAsync(Guid id, bool deactivateChildren = true)
    {
        var service = await _serviceRepo.GetByIdWithHierarchyAsync(id)
            ?? throw new NotFoundException($"Service with ID {id} not found");

        service.IsActive = false;

        if (deactivateChildren)
        {
            foreach (var category in service.Categories)
            {
                category.IsActive = false;
                foreach (var subcategory in category.Subcategories)
                {
                    subcategory.IsActive = false;
                }
            }
        }

        await _serviceRepo.UpdateAsync(service);
        _logger.LogInformation("Deactivated service {ServiceId}, children: {DeactivateChildren}", id, deactivateChildren);
    }

    // ========== VALIDATION OPERATIONS ==========

    public async Task<bool> IsServiceNameUniqueAsync(string name, Guid? excludeServiceId = null)
    {
        return await _serviceRepo.IsNameUniqueAsync(name, excludeServiceId);
    }

    public async Task<bool> ValidateReferencesAsync(Guid? industryId, Guid? intakeFormId)
    {
        // Implement validation logic for Industry and IntakeForm references
        // This would typically involve checking if the referenced entities exist
        return await _serviceRepo.ValidateReferencesAsync(industryId, intakeFormId);
    }

    // ========== CATEGORY OPERATIONS ==========

    public async Task<ServiceResponseDto> AddCategoryAsync(Guid serviceId, CreateServiceCategoryDto categoryDto)
    {
        var service = await _serviceRepo.GetByIdAsync(serviceId)
            ?? throw new NotFoundException($"Service with ID {serviceId} not found");

        var category = _mapper.Map<ServiceCategory>(categoryDto);
        category.Id = Guid.NewGuid();
        category.ServiceId = serviceId;

        await _categoryRepo.AddAsync(category);
        _logger.LogInformation("Added category {CategoryId} to service {ServiceId}", category.Id, serviceId);

        return await GetByIdAsync(serviceId);
    }

    public async Task<ServiceResponseDto> UpdateCategoryAsync(Guid serviceId, Guid categoryId, UpdateServiceCategoryDto categoryDto)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId)
            ?? throw new NotFoundException($"Category with ID {categoryId} not found");

        if (category.ServiceId != serviceId)
            throw new ValidationException(["Category does not belong to the specified service"]);

        _mapper.Map(categoryDto, category);
        await _categoryRepo.UpdateAsync(category);

        _logger.LogInformation("Updated category {CategoryId} in service {ServiceId}", categoryId, serviceId);
        return await GetByIdAsync(serviceId);
    }

    public async Task<ServiceResponseDto> RemoveCategoryAsync(Guid serviceId, Guid categoryId)
    {
        var category = await _categoryRepo.GetByIdWithSubcategoriesAsync(categoryId)
            ?? throw new NotFoundException($"Category with ID {categoryId} not found");

        if (category.ServiceId != serviceId)
            throw new ValidationException(["Category does not belong to the specified service"]);

        await _categoryRepo.DeleteAsync(category);
        _logger.LogInformation("Removed category {CategoryId} from service {ServiceId}", categoryId, serviceId);

        return await GetByIdAsync(serviceId);
    }

    // ========== BUSINESS LOGIC OPERATIONS ==========

    public async Task<decimal?> GetEffectivePriceAsync(Guid serviceId, Guid? categoryId = null, Guid? subcategoryId = null)
    {
        if (subcategoryId.HasValue)
        {
            var subcategory = await _subcategoryRepo.GetByIdAsync(subcategoryId.Value);
            return subcategory?.Price;
        }

        if (categoryId.HasValue)
        {
            var category = await _categoryRepo.GetByIdAsync(categoryId.Value);
            return category?.Price;
        }

        var service = await _serviceRepo.GetByIdAsync(serviceId);
        return service?.PricePerPerson;
    }

    public async Task<int> GetTotalDurationAsync(Guid serviceId)
    {
        var service = await _serviceRepo.GetByIdWithHierarchyAsync(serviceId)
            ?? throw new NotFoundException($"Service with ID {serviceId} not found");

        return service.Categories
            .SelectMany(c => c.Subcategories)
            .Sum(s => s.DurationMinutes ?? 0);
    }

    public async Task<bool> HasActiveBookingsAsync(Guid serviceId)
    {
        // This would integrate with your booking system
        // For now, return false as a placeholder
        return await _serviceRepo.HasActiveBookingsAsync(serviceId);
    }

    // ========== PRIVATE HELPER METHODS ==========

    private async Task UpdateCategoriesWithReplaceSemantics(Service service, List<UpdateServiceCategoryDto> categoryDtos)
    {
        var existingCategories = service.Categories.ToList();
        var inputCategoryIds = categoryDtos.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToList();

        // Delete categories not in the input
        var categoriesToDelete = existingCategories.Where(c => !inputCategoryIds.Contains(c.Id)).ToList();
        foreach (var category in categoriesToDelete)
        {
            await _categoryRepo.DeleteAsync(category);
        }

        // Process each category in the input
        foreach (var categoryDto in categoryDtos)
        {
            if (categoryDto.Id.HasValue)
            {
                // Update existing category
                var existingCategory = existingCategories.FirstOrDefault(c => c.Id == categoryDto.Id.Value);
                if (existingCategory != null)
                {
                    _mapper.Map(categoryDto, existingCategory);
                    await _categoryRepo.UpdateAsync(existingCategory);
                    await UpdateSubcategoriesWithReplaceSemantics(existingCategory, categoryDto.Subcategories);
                }
            }
            else
            {
                // Create new category
                var newCategory = _mapper.Map<ServiceCategory>(categoryDto);
                newCategory.Id = Guid.NewGuid();
                newCategory.ServiceId = service.Id;
                await _categoryRepo.AddAsync(newCategory);

                // Create subcategories
                foreach (var subcategoryDto in categoryDto.Subcategories)
                {
                    var newSubcategory = _mapper.Map<ServiceSubcategory>(subcategoryDto);
                    newSubcategory.Id = Guid.NewGuid();
                    newSubcategory.ServiceCategoryId = newCategory.Id;
                    await _subcategoryRepo.AddAsync(newSubcategory);
                }
            }
        }
    }

    private async Task UpdateSubcategoriesWithReplaceSemantics(ServiceCategory category, List<UpdateServiceSubcategoryDto> subcategoryDtos)
    {
        var existingSubcategories = category.Subcategories.ToList();
        var inputSubcategoryIds = subcategoryDtos.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToList();

        // Delete subcategories not in the input
        var subcategoriesToDelete = existingSubcategories.Where(s => !inputSubcategoryIds.Contains(s.Id)).ToList();
        foreach (var subcategory in subcategoriesToDelete)
        {
            await _subcategoryRepo.DeleteAsync(subcategory);
        }

        // Process each subcategory in the input
        foreach (var subcategoryDto in subcategoryDtos)
        {
            if (subcategoryDto.Id.HasValue)
            {
                // Update existing subcategory
                var existingSubcategory = existingSubcategories.FirstOrDefault(s => s.Id == subcategoryDto.Id.Value);
                if (existingSubcategory != null)
                {
                    _mapper.Map(subcategoryDto, existingSubcategory);
                    await _subcategoryRepo.UpdateAsync(existingSubcategory);
                }
            }
            else
            {
                // Create new subcategory
                var newSubcategory = _mapper.Map<ServiceSubcategory>(subcategoryDto);
                newSubcategory.Id = Guid.NewGuid();
                newSubcategory.ServiceCategoryId = category.Id;
                await _subcategoryRepo.AddAsync(newSubcategory);
            }
        }
    }
    public async Task AssignFormAsync(AssignFormToServiceDto dto)
    {
        var service = await _serviceRepo.GetByIdAsync(dto.ServiceId)
            ?? throw new NotFoundException("Service not found");

        var form = await _formRepo.GetByIdAsync(dto.FormId)
            ?? throw new NotFoundException("Form not found");

        service.IntakeFormId = form.Id;

        await _serviceRepo.UpdateAsync(service);
        // await _serviceRepo.SaveChangesAsync();
    }

}
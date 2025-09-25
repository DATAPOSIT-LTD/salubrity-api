using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Shared.Exceptions;

public class PackageReferenceResolverService : IPackageReferenceResolver
{
    private readonly IServiceRepository _serviceRepo;
    private readonly IServiceCategoryRepository _categoryRepo;
    private readonly IServiceSubcategoryRepository _subcategoryRepo;

    public PackageReferenceResolverService(
        IServiceRepository serviceRepo,
        IServiceCategoryRepository categoryRepo,
        IServiceSubcategoryRepository subcategoryRepo)
    {
        _serviceRepo = serviceRepo;
        _categoryRepo = categoryRepo;
        _subcategoryRepo = subcategoryRepo;
    }

    public async Task<PackageItemType> ResolveTypeAsync(Guid referenceId)
    {
        if (await _serviceRepo.ExistsByIdAsync(referenceId))
            return PackageItemType.Service;

        if (await _categoryRepo.ExistsByIdAsync(referenceId))
            return PackageItemType.ServiceCategory;

        if (await _subcategoryRepo.ExistsByIdAsync(referenceId))
            return PackageItemType.ServiceSubcategory;

        throw new ValidationException(["Invalid ReferenceId: No matching service, category, or subcategory found."]);
    }

    public async Task<string> GetNameAsync(PackageItemType type, Guid referenceId)
    {
        return type switch
        {
            PackageItemType.Service => (await _serviceRepo.GetByIdAsync(referenceId))?.Name ?? "[Service missing]",
            PackageItemType.ServiceCategory => (await _categoryRepo.GetByIdAsync(referenceId))?.Name ?? "[Category missing]",
            PackageItemType.ServiceSubcategory => (await _subcategoryRepo.GetByIdAsync(referenceId))?.Name ?? "[Subcategory missing]",
            _ => "[Unknown type]"
        };
    }

    public async Task<string?> GetDescriptionAsync(PackageItemType type, Guid referenceId)
    {
        return type switch
        {
            PackageItemType.Service => (await _serviceRepo.GetByIdAsync(referenceId))?.Description,
            PackageItemType.ServiceCategory => (await _categoryRepo.GetByIdAsync(referenceId))?.Description,
            PackageItemType.ServiceSubcategory => (await _subcategoryRepo.GetByIdAsync(referenceId))?.Description,
            _ => null
        };
    }


    // public async Task<Service?> ResolveServiceAsync(Guid assignmentId, PackageItemType type)
    // {
    //     return type switch
    //     {
    //         PackageItemType.Service => await _serviceRepo.GetByIdAsync(assignmentId),

    //         PackageItemType.ServiceCategory =>
    //             (await _categoryRepo.GetByIdAsync(assignmentId))?.Service,

    //         PackageItemType.ServiceSubcategory =>
    //             (await _subcategoryRepo.GetByIdAsync(assignmentId))?.ServiceCategory?.Service,

    //         _ => throw new ValidationException(["Invalid PackageItemType when resolving service."])
    //     };
    // }
    public async Task<Service?> ResolveServiceAsync(Guid assignmentId, PackageItemType type)
    {
        switch (type)
        {
            case PackageItemType.Service:
                var service = await _serviceRepo.GetByIdWithCategoriesAsync(assignmentId);
                if (service == null)
                    return null;

                // If the service itself has an IntakeForm → use it
                if (service.IntakeFormId != null && service.IsActive)
                    return service;

                // Otherwise, try categories
                var activeCategoryWithForm = service.Categories
                    .FirstOrDefault(c => c.IsActive && c.IntakeFormId != null);

                if (activeCategoryWithForm != null)
                    return service; // still return the parent, but note: form comes from category

                return service; // fallback (even if inactive)

            case PackageItemType.ServiceCategory:
                var category = await _categoryRepo.GetByIdAsync(assignmentId);
                return category?.Service;

            case PackageItemType.ServiceSubcategory:
                var subcategory = await _subcategoryRepo.GetByIdAsync(assignmentId);
                return subcategory?.ServiceCategory?.Service;

            default:
                throw new ValidationException(["Invalid PackageItemType when resolving service."]);
        }
    }



    public async Task<Guid> ResolveServiceIdAsync(Guid referenceId)
    {
        // 1. If it's a Service → return as-is
        if (await _serviceRepo.ExistsByIdAsync(referenceId))
            return referenceId;

        // 2. If it's a Category → return .ServiceId
        var category = await _categoryRepo.GetByIdAsync(referenceId);
        if (category != null)
            return category.ServiceId;

        // 3. If it's a Subcategory → get .Category → then .ServiceId
        var subcategory = await _subcategoryRepo.GetByIdAsync(referenceId);
        if (subcategory != null)
        {
            var parentCategory = await _categoryRepo.GetByIdAsync(subcategory.ServiceCategoryId);
            if (parentCategory == null)
                throw new ValidationException(["Subcategory's parent category not found."]);
            return parentCategory.ServiceId;
        }

        // 4. Nothing matched
        throw new ValidationException(["Invalid ReferenceId: No matching service, category, or subcategory found."]);
    }

    public async Task<(Guid? ParentId, PackageItemType? ParentType)> GetParentAsync(Guid referenceId)
    {
        // 1. If Service → no parent
        if (await _serviceRepo.ExistsByIdAsync(referenceId))
            return (null, null);

        // 2. If Category → parent is Service
        var category = await _categoryRepo.GetByIdAsync(referenceId);
        if (category != null)
            return (category.ServiceId, PackageItemType.Service);

        // 3. If Subcategory → parent is Category
        var subcategory = await _subcategoryRepo.GetByIdAsync(referenceId);
        if (subcategory != null)
            return (subcategory.ServiceCategoryId, PackageItemType.ServiceCategory);

        // 4. No match
        throw new ValidationException(["Invalid ReferenceId: No matching service, category, or subcategory found."]);
    }



}

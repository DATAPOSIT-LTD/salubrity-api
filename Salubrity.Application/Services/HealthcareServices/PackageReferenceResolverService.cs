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


    public async Task<Service?> ResolveServiceAsync(Guid assignmentId, PackageItemType type)
    {
        return type switch
        {
            PackageItemType.Service => await _serviceRepo.GetByIdAsync(assignmentId),

            PackageItemType.ServiceCategory =>
                (await _categoryRepo.GetByIdAsync(assignmentId))?.Service,

            PackageItemType.ServiceSubcategory =>
                (await _subcategoryRepo.GetByIdAsync(assignmentId))?.ServiceCategory?.Service,

            _ => throw new ValidationException(["Invalid PackageItemType when resolving service."])
        };
    }


}

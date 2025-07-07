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
}

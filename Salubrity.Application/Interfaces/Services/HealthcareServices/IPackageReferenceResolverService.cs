using Salubrity.Domain.Entities.HealthcareServices;
namespace Salubrity.Application.Interfaces.Services.HealthcareServices;

public interface IPackageReferenceResolver
{
    Task<PackageItemType> ResolveTypeAsync(Guid referenceId);
    Task<string> GetNameAsync(PackageItemType type, Guid referenceId);
    Task<string?> GetDescriptionAsync(PackageItemType type, Guid referenceId);
    Task<Service?> ResolveServiceAsync(Guid assignmentId, PackageItemType type);
    Task<Guid> ResolveServiceIdAsync(Guid referenceId);
    Task<(Guid? ParentId, PackageItemType? ParentType)> GetParentAsync(Guid referenceId);

}

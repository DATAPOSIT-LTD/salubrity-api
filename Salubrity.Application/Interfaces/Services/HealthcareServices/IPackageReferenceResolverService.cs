using Salubrity.Domain.Entities.HealthcareServices;
namespace Salubrity.Application.Interfaces.Services.HealthcareServices;

public interface IPackageReferenceResolver
{
    Task<PackageItemType> ResolveTypeAsync(Guid referenceId);
    Task<string> GetNameAsync(PackageItemType type, Guid referenceId);

}

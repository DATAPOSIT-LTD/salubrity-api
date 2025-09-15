using AutoMapper;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;

public class AssignmentNameResolver : IValueResolver<HealthCampServiceAssignment, HealthCampServiceAssignmentDto, string?>
{
    private readonly IPackageReferenceResolver _resolver;

    public AssignmentNameResolver(IPackageReferenceResolver resolver)
    {
        _resolver = resolver;
    }

    public string? Resolve(
        HealthCampServiceAssignment source,
        HealthCampServiceAssignmentDto destination,
        string? destMember,
        ResolutionContext context)
    {
        return _resolver
            .GetNameAsync((PackageItemType)source.AssignmentType, source.AssignmentId)
            .GetAwaiter()
            .GetResult(); // 
    }
}

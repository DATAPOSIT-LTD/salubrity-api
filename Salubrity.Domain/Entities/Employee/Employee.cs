// File: Salubrity.Domain.Entities.Identity.Employee.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Organizations;

namespace Salubrity.Domain.Entities.Identity;

public class Employee : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid? JobTitleId { get; set; }
    public JobTitle? JobTitle { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
}

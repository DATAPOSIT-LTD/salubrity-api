// File: Salubrity.Domain.Entities.Identity.Employee.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Organizations;

namespace Salubrity.Domain.Entities.Identity;

public class Employee : BaseAuditableEntity
{
	public Guid UserId { get; set; }
	public User User { get; set; } = null!;

	public Guid OrganizationId { get; set; }
	public Organization Organization { get; set; } = null!;

	public string? JobTitle { get; set; }
	public string? Department { get; set; }
}

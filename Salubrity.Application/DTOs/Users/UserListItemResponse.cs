using Salubrity.Application.DTOs.Organizations;

namespace Salubrity.Application.DTOs.Users;

public class UserListItemResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string? Phone { get; set; }

    public List<UserRoleSummary> Roles { get; set; } = new();

    public OrganizationSummary? Organization { get; set; }
}



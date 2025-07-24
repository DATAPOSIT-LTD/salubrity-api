using Salubrity.Application.DTOs.Users;

namespace Salubrity.Application.DTOs.Employees;

public class EmployeeResponseDto
{
    internal string JobTitle;
    internal string Department;

    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = default!;

    public Guid? JobTitleId { get; set; }
    public string? JobTitleName { get; set; }

    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    public UserResponse User { get; set; } = new();
}

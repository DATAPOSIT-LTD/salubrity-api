using Salubrity.Application.DTOs.Users;

namespace Salubrity.Application.DTOs.Employees;

public class EmployeeRequestDto
{
    public Guid OrganizationId { get; set; }

    public Guid? JobTitleId { get; set; }

    public Guid? DepartmentId { get; set; }

    public UserCreateRequest User { get; set; } = new();
}

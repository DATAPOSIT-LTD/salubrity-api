using Salubrity.Application.DTOs.Users;

namespace Salubrity.Application.DTOs.Employees;

public class EmployeeResponseDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = default!;
    public string? JobTitle { get; set; }
    public string? Department { get; set; } //prmitive
    public  UserResponse User { get; set; } = new();

}
using Salubrity.Application.DTOs.Users;

namespace Salubrity.Application.DTOs.Employees;

public class EmployeeRequestDto
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public  UserResponse User { get; set; } = new();

   
}
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




public class EmployeeLeanResponseDto
{

    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PrimaryLanguage { get; set; }
    public string? ProfileImage { get; set; }
    public Guid? GenderId { get; set; }
    public Guid? OrganizationId { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ");
}
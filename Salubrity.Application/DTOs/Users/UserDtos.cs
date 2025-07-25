using System;

namespace Salubrity.Application.DTOs.Users;

public class UserResponse
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

public class UserCreateRequest
{
    public string Email { get; set; } = null!;
    public string? Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public Guid? GenderId { get; set; }
    public Guid? OrganizationId { get; set; }
}

public class UserUpdateRequest
{
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string? PrimaryLanguage { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

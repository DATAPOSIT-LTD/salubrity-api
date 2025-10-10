public class RegisterRequestDto
{
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
    public string? CampToken { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? RoleId { get; set; } // Or string RoleName, depending on implementation
    public bool AcceptTerms { get; set; } // Optional, if you want to enforce checkbox logic
}

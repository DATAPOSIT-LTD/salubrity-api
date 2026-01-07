public class RegisterRequestDto
{
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;

    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;

    // Camp resolution
    public string? CampSlug { get; set; }

    // Organization fallback (non-camp flows)
    public Guid? OrganizationId { get; set; }

    // Role resolution (mutually inclusive, precedence rules apply)
    public Guid? RoleId { get; set; }
    public string? Role { get; set; } // "participant" | "subcontractor"

    public bool AcceptTerms { get; set; }
}

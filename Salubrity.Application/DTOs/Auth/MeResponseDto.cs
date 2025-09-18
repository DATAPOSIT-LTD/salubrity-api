using Salubrity.Application.DTOs.Menus;

public class MeResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
    public List<MenuResponseDto> Menus { get; set; } = [];

    // Add these:
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool OnboardingComplete { get; set; }
    public BillingStatusDto? BillingStatus { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class BillingStatusDto
{
    public bool CanProceed { get; set; }
    public string? Status { get; set; }
}
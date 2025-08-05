public class EmployeeCsvRowDto
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? NationalId { get; set; }
    public string? PrimaryLanguage { get; set; }
    public string JobTitleName { get; set; } = default!;
    public string DepartmentName { get; set; } = default!;
    public string OrganizationName { get; set; } = default!;

}

public class HealthCampWithRolesDto
{
    public Guid CampId { get; set; }
    public string ClientName { get; set; } = default!;
    public string Venue { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = default!;
    public List<RoleAssignmentDto> Roles { get; set; } = new();
}

public class RoleAssignmentDto
{
    public string AssignedBooth { get; set; } = default!;
    public string AssignedRole { get; set; } = default!;
    public Guid ServiceId { get; set; }
}

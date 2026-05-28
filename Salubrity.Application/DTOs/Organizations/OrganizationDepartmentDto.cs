namespace Salubrity.Application.DTOs.Organizations;

public class OrganizationDepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateOrgDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class BulkDepartmentUploadResultDto
{
    public int Added { get; set; }
    public int Skipped { get; set; }
    public List<string> SkippedReasons { get; set; } = new();
}

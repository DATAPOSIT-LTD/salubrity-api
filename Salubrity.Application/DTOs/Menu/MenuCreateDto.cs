namespace Salubrity.Application.DTOs.Menus
{
    public class MenuCreateDto
    {
        public string Label { get; set; } = default!;
        public string Path { get; set; } = default!;
        public string? Icon { get; set; }
        public int Order { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? RequiredPermissionId { get; set; }
        public bool IsActive { get; set; } = true;

        public List<Guid>? RoleIds { get; set; }
    }
}

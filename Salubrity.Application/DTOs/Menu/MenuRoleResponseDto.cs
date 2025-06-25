namespace Salubrity.Application.DTOs.Menus
{
    public class MenuRoleResponseDto
    {
        public Guid Id { get; set; }
        public Guid MenuId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}

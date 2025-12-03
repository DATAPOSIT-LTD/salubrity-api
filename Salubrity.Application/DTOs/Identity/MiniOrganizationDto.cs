using System;

namespace Salubrity.Application.DTOs.Identity
{
    public class MiniOrganizationDto
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; } = null!;
    }
}

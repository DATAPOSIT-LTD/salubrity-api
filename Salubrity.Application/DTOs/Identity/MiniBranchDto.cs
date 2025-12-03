using System;

namespace Salubrity.Application.DTOs.Identity
{
    public class MiniBranchDto
    {
        public Guid Id { get; set; }
        public string BranchName { get; set; } = null!;
    }
}

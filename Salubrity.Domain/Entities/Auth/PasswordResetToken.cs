using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Auth
{
    [Table("PasswordResetTokens")]
    public class PasswordResetToken : BaseAuditableEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;

        [Required, MaxLength(256)]
        public string Token { get; set; } = default!;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;
    }
}

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Lookup;
using System;

namespace Salubrity.Domain.Entities.Join
{
    public class UserLanguage : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid LanguageId { get; set; }
        public Language Language { get; set; } = null!;
        public bool IsPrimary { get; set; } = false;
    }
}

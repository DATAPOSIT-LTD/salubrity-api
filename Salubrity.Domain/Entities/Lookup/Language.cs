using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Join;
using System.Collections.Generic;

namespace Salubrity.Domain.Entities.Lookup
{
    public class Language : BaseLookupEntity
    {
        public ICollection<User>? Users { get; set; } = new List<User>();
        public ICollection<UserLanguage> UserLanguages { get; set; } = new List<UserLanguage>();

    }
}

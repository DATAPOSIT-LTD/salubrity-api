using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Identity;
using System.Collections.Generic;

namespace Salubrity.Domain.Entities.Lookup
{
    public class Gender : BaseLookupEntity
    {
        public ICollection<User> Users { get; set; } = [];
    }
}

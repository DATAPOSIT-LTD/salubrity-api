using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Identity;
using System.Collections.Generic;

namespace Salubrity.Domain.Entities.Lookup
{
    public class JobTitle : BaseLookupEntity
    {
        public ICollection<Employee> Employees { get; set; } = [];
    }
}

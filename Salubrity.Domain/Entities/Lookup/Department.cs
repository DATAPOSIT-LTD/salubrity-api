using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Identity;
using System.Collections.Generic;

namespace Salubrity.Domain.Entities.Lookup
{
    public class Department : BaseLookupEntity
    {
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}

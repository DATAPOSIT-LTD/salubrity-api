using System.Collections.Generic;

namespace Salubrity.Domain.Common
{
    public interface IHasDomainEvents
    {
        List<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }
}

using System;
using System.Collections.Generic;

namespace Salubrity.Domain.Common
{
    /// <summary>
    /// Represents a base class for all entities that require auditing, soft deletion, and domain events.
    /// </summary>
    public abstract class BaseAuditableEntity : IHasDomainEvents
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// UTC timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional UTC timestamp when the entity was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Optional UTC timestamp when the entity was soft deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Flag indicating whether the entity has been soft deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// ID of the user who created the entity.
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// ID of the user who last updated the entity.
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// ID of the user who soft deleted the entity.
        /// </summary>
        public Guid? DeletedBy { get; set; }

        /// <summary>
        /// List of domain events raised by the entity.
        /// </summary>
        private readonly List<IDomainEvent> _domainEvents = [];

        /// <inheritdoc />
        public List<IDomainEvent> DomainEvents => _domainEvents;

        /// <inheritdoc />
        public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        /// <inheritdoc />
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}

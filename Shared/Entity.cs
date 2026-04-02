using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared.Events;

namespace UnceasingFear.Domain.Shared
{
    public abstract class Entity
    {
        private readonly List<IDomainEvent> _events = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _events.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent e) => _events.Add(e);

        public IReadOnlyList<IDomainEvent> PullDomainEvents()
        {
            var copy = _events.ToList();
            _events.Clear();
            return copy;
        }
    }
}

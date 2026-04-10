
namespace UnceasingFear.Domain.Shared.Events
{
    public interface IDomainEvent { }
    public interface IEventDispatcher { 
        public void Subscribe<T>(Action<T> handler); 
        public void Dispatch(IDomainEvent e); 
    }

    public class DomainEventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, List<Action<IDomainEvent>>> _handlers = new();

        void IEventDispatcher.Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new();
            _handlers[type].Add(e => handler((T)e));
        }

        public void Dispatch(IDomainEvent e)
        {
            if (_handlers.TryGetValue(e.GetType(), out var handlers))
                foreach (var h in handlers) h(e);
        }
    }
}

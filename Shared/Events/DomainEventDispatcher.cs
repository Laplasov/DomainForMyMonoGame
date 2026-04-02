
namespace UnceasingFear.Domain.Shared.Events
{
    public interface IDomainEvent { }
    public class DomainEventDispatcher
    {
        private readonly Dictionary<Type, List<Action<IDomainEvent>>> _handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : IDomainEvent
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnceasingFear.Application.Commands
{
    public interface ICommandDispatcher
    {
        void Dispatch<T>(T command);
        void Unsubscribe<T>();
        void Register<T>(Action<T> handler);
    }

    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<Type, Action<object>> _handlers = new();
        public void Register<T>(Action<T> handler) =>
            _handlers[typeof(T)] = cmd => handler((T)cmd);
        public void Unsubscribe<T>() => _handlers.Remove(typeof(T));
        public void Dispatch<T>(T command)
        {
            if (_handlers.TryGetValue(typeof(T), out var handler))
                handler(command!);
        }
    }
}

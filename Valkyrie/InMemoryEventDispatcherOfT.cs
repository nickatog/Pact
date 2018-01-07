using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie
{
    internal sealed class InMemoryEventDispatcher<T>
        : IEventDispatcher<T>
    {
        private readonly IDictionary<Type, IList<IEventHandler<T>>> _handlersByType = new Dictionary<Type, IList<IEventHandler<T>>>();

        IEnumerable<T> IEventDispatcher<T>.DispatchEvent(
            object @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var results = Enumerable.Empty<T>();

            IList<IEventHandler<T>> handlersForType = null;
            if (!_handlersByType.TryGetValue(@event.GetType(), out handlersForType))
                return results;

            var handlersToInvoke = new IEventHandler<T>[handlersForType.Count];
            handlersForType.CopyTo(handlersToInvoke, 0);

            foreach (IEventHandler<T> handler in handlersToInvoke)
                results = results.Concat(handler.HandleEvent(@event));

            return results;
        }

        void IEventDispatcher<T>.RegisterHandler(
            IEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type eventType = handler.EventType;

            IList<IEventHandler<T>> handlersForType = null;
            if (!_handlersByType.TryGetValue(eventType, out handlersForType))
            {
                handlersForType = new List<IEventHandler<T>>();

                _handlersByType.Add(eventType, handlersForType);
            }

            handlersForType.Add(handler);
        }

        void IEventDispatcher<T>.UnregisterHandler(
            IEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            IList<IEventHandler<T>> handlersForType = null;
            if (_handlersByType.TryGetValue(handler.EventType, out handlersForType))
                handlersForType.Remove(handler);
        }
    }
}

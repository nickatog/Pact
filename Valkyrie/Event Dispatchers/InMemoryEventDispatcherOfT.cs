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

            if (!_handlersByType.TryGetValue(@event.GetType(), out IList<IEventHandler<T>> handlersForType))
                return results;

            var handlersToInvoke = new IEventHandler<T>[handlersForType.Count];
            handlersForType.CopyTo(handlersToInvoke, 0);

            return handlersToInvoke.SelectMany(__handler => __handler.HandleEvent(@event)).ToList();
        }

        void IEventDispatcher<T>.RegisterHandler(
            IEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type eventType = handler.EventType;

            if (!_handlersByType.TryGetValue(eventType, out IList<IEventHandler<T>> handlersForType))
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

            if (_handlersByType.TryGetValue(handler.EventType, out IList<IEventHandler<T>> handlersForType))
                handlersForType.Remove(handler);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Valkyrie
{
    internal sealed class InMemoryEventDispatcher
        : IEventDispatcher
    {
        private readonly IDictionary<Type, HashSet<IEventHandler>> _handlersByType =
            new Dictionary<Type, HashSet<IEventHandler>>();

        void IEventDispatcher.DispatchEvent(
            object @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (!_handlersByType.TryGetValue(@event.GetType(), out HashSet<IEventHandler> handlersForType))
                return;

            var handlersToInvoke = new IEventHandler[handlersForType.Count];
            handlersForType.CopyTo(handlersToInvoke, 0);

            foreach (IEventHandler handler in handlersToInvoke)
                handler.HandleEvent(@event);
        }

        void IEventDispatcher.RegisterHandler(
            IEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type eventType = handler.EventType;
            
            if (!_handlersByType.TryGetValue(eventType, out HashSet<IEventHandler> handlersForType))
            {
                handlersForType = new HashSet<IEventHandler>();

                _handlersByType.Add(eventType, handlersForType);
            }
            
            handlersForType.Add(handler);
        }

        void IEventDispatcher.UnregisterHandler(
            IEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (_handlersByType.TryGetValue(handler.EventType, out HashSet<IEventHandler> handlersForType))
                handlersForType.Remove(handler);
        }
    }
}

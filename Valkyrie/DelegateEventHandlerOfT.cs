using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie
{
    public sealed class DelegateEventHandler<TEvent, T>
        : IEventHandler<T>
        where TEvent : class
    {
        private readonly Func<TEvent, IEnumerable<T>> _handlerFunc = null;

        public DelegateEventHandler(
            Func<TEvent, IEnumerable<T>> handlerFunc)
        {
            if (handlerFunc == null)
                throw new ArgumentNullException(nameof(handlerFunc));

            _handlerFunc = handlerFunc;
        }

        Type IEventHandler<T>.EventType => typeof(TEvent);

        IEnumerable<T> IEventHandler<T>.HandleEvent(
            object @event)
        {
            var realEvent = @event as TEvent;
            if (realEvent == null)
                return Enumerable.Empty<T>();

            return _handlerFunc(realEvent);
        }
    }
}

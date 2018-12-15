using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie
{
    public sealed class DelegateEventHandler<TEvent, T>
        : IEventHandler<T>
        where TEvent : class
    {
        private readonly Func<TEvent, IEnumerable<T>> _handlerFunc;

        public DelegateEventHandler(
            Func<TEvent, IEnumerable<T>> handlerFunc)
        {
            _handlerFunc = handlerFunc ?? throw new ArgumentNullException(nameof(handlerFunc));
        }

        Type IEventHandler<T>.EventType => typeof(TEvent);

        IEnumerable<T> IEventHandler<T>.HandleEvent(
            object @event)
        {
            if (@event is TEvent realEvent)
                return _handlerFunc(realEvent);

            return Enumerable.Empty<T>();
        }
    }
}

using System;

namespace Valkyrie
{
    public sealed class DelegateEventHandler<TEvent>
        : IEventHandler
        where TEvent : class
    {
        private readonly Action<TEvent> _handlerAction;

        public DelegateEventHandler(
            Action<TEvent> handlerAction)
        {
            _handlerAction = handlerAction ?? throw new ArgumentNullException(nameof(handlerAction));
        }

        Type IEventHandler.EventType => typeof(TEvent);

        void IEventHandler.HandleEvent(
            object @event)
        {
            if (@event is TEvent realEvent)
                _handlerAction(realEvent);
        }
    }
}

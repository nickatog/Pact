using System;

namespace Valkyrie
{
    public sealed class DelegateEventHandler<TEvent>
        : IEventHandler
        where TEvent : class
    {
        private readonly Action<TEvent> _handlerAction = null;

        public DelegateEventHandler(
            Action<TEvent> handlerAction)
        {
            if (handlerAction == null)
                throw new ArgumentNullException(nameof(handlerAction));

            _handlerAction = handlerAction;
        }

        Type IEventHandler.EventType => typeof(TEvent);

        void IEventHandler.HandleEvent(
            object @event)
        {
            var realEvent = @event as TEvent;
            if (realEvent == null)
                return;

            _handlerAction(realEvent);
        }
    }
}

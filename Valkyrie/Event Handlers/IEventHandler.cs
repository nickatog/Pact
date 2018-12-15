using System;

namespace Valkyrie
{
    public interface IEventHandler
    {
        Type EventType { get; }

        void HandleEvent(
            object @event);
    }
}

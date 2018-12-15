using System.Collections.Generic;

namespace Valkyrie
{
    public interface IEventDispatcher<T>
    {
        IEnumerable<T> DispatchEvent(
            object @event);

        void RegisterHandler(
            IEventHandler<T> handler);

        void UnregisterHandler(
            IEventHandler<T> handler);
    }
}

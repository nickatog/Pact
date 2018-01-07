using System;
using System.Collections.Generic;

namespace Valkyrie
{
    public interface IEventHandler<T>
    {
        Type EventType { get; }

        IEnumerable<T> HandleEvent(
            object @event);
    }
}

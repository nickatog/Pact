using System;

namespace Pact
{
    public interface IModalViewModel<TResult>
    {
        event Action<TResult> OnClosed;
    }
}

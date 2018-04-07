using System;

namespace Pact
{
    public interface IModalViewModel
    { }

    public interface IModalViewModel<TResult>
        : IModalViewModel
    {
        event Action<TResult> OnClosed;
    }
}

using System;

namespace Pact
{
    public interface IModalDisplay
    {
        void Show<TResult>(
            IModalViewModel<TResult> viewModel,
            Action<TResult> onClosed,
            int fadeDuration = 0);
    }
}

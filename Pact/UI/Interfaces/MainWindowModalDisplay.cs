using System;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class MainWindowModalDisplay
        : IModalDisplay
    {
        void IModalDisplay.Show<TResult>(
            IModalViewModel<TResult> viewModel,
            Action<TResult> onClosed,
            int fadeDuration)
        {
            viewModel.Require(nameof(viewModel));

            MainWindowViewModel.Instance.SetModalViewModel(viewModel, fadeDuration);

            viewModel.OnClosed += onClosed;
        }
    }
}

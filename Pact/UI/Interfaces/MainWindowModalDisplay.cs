using System;

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
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            MainWindowViewModel.Instance.SetModalViewModel(viewModel, fadeDuration);

            viewModel.OnClosed += onClosed;
        }
    }
}

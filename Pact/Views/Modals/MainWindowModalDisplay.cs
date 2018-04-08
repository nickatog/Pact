using System;

namespace Pact
{
    public sealed class MainWindowModalDisplay
        : IModalDisplay
    {
        void IModalDisplay.Show<TResult>(
            IModalViewModel<TResult> viewModel,
            Action<TResult> onClosed)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            MainWindowViewModel.Instance.SetModalViewModel(viewModel);

            viewModel.OnClosed += onClosed;
        }
    }
}

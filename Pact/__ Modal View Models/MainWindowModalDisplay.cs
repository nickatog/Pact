using System;

namespace Pact
{
    public sealed class ModalDisplay
        : IModalDisplay
    {
        void IModalDisplay.Show<TResult>(
            IModalViewModel<TResult> viewModel,
            Action<TResult> onClosed)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            viewModel.OnClosed += onClosed;

            MainWindowViewModel.Instance.SetModalViewModel(viewModel);
        }
    }
}

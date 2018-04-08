using System;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalDisplay
        : IModalDisplay
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ModalDisplay(
            MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel.Require(nameof(mainWindowViewModel));
        }

        void IModalDisplay.Show<TResult>(
            IModalViewModel<TResult> viewModel,
            Action<TResult> onClosed)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            viewModel.OnClosed += onClosed;

            _mainWindowViewModel.SetModalViewModel(viewModel);
        }
    }
}

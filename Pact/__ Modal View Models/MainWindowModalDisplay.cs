using System;

namespace Pact
{
    public sealed class ModalDisplay
        : IModalDisplay
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ModalDisplay(
            MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel ?? throw new ArgumentNullException(nameof(mainWindowViewModel));
        }

        void IModalDisplay.Show<TViewModel>(
            IModalViewModel<TViewModel> viewModel,
            Action<TViewModel> onClosed)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            viewModel.OnClosed += onClosed;

            _mainWindowViewModel.SetModalViewModel(viewModel);
        }
    }
}

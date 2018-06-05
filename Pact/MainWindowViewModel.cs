#region Namespaces
using System;
using System.ComponentModel;
using System.Windows.Input;
#endregion // Namespaces

namespace Pact
{
    public sealed class MainWindowViewModel
        : INotifyPropertyChanged
    {
        #region Dependencies
        private readonly ConfigurationSettingsViewModel _configurationSettingsViewModel;
        private readonly DeckManagerViewModel _deckManagerViewModel;
        #endregion // Dependencies

        #region Fields
        private static MainWindowViewModel _instance;
        public static MainWindowViewModel Instance => _instance;

        private object _modalViewModel;
        private object _viewModel;
        #endregion // Fields

        #region Constructors
        public MainWindowViewModel(
            ConfigurationSettingsViewModel configurationSettingsViewModel,
            DeckManagerViewModel deckManagerViewModel)
        {
            _configurationSettingsViewModel =
                configurationSettingsViewModel
                ?? throw new ArgumentNullException(nameof(configurationSettingsViewModel));

            _deckManagerViewModel =
                deckManagerViewModel
                ?? throw new ArgumentNullException(nameof(deckManagerViewModel));

            _viewModel = _deckManagerViewModel;

            _instance = this;
        }
        #endregion // Constructors

        public object ModalViewModel
        {
            get => _modalViewModel;
            private set
            {
                _modalViewModel = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModalViewModel)));
            }
        }

        public void SetModalViewModel<TResult>(
            IModalViewModel<TResult> viewModel)
        {
            viewModel.OnClosed +=
                __ => ModalViewModel = null;

            ModalViewModel = viewModel;
        }

        public ICommand ShowConfigurationSettings =>
            new DelegateCommand(
                () => ViewModel = _configurationSettingsViewModel);

        public ICommand ShowDeckManager =>
            new DelegateCommand(
                () => ViewModel = _deckManagerViewModel);

        public object ViewModel
        {
            get => _viewModel;
            private set
            {
                _viewModel = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewModel)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

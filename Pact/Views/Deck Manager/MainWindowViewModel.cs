using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class MainWindowViewModel
        : INotifyPropertyChanged
    {
        private readonly ConfigurationSettingsViewModel _configurationSettingsViewModel;
        private readonly DeckManagerViewModel _deckManagerViewModel;

        private object _viewModel;

        private static MainWindowViewModel _instance;
        public static MainWindowViewModel Instance => _instance;

        public MainWindowViewModel(
            DeckManagerViewModel deckManagerViewModel,
            ConfigurationSettingsViewModel configurationSettingsViewModel)
        {
            _configurationSettingsViewModel = configurationSettingsViewModel ?? throw new ArgumentNullException(nameof(configurationSettingsViewModel));
            _deckManagerViewModel = deckManagerViewModel ?? throw new ArgumentNullException(nameof(deckManagerViewModel));

            _viewModel = _deckManagerViewModel;

            _instance = this;
        }

        public object ModalViewModel { get; private set; }

        public void SetModalViewModel<TResult>(
            IModalViewModel<TResult> viewModel)
        {
            viewModel.OnClosed +=
                __ =>
                {
                    ModalViewModel = null;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModalViewModel)));
                };

            ModalViewModel = viewModel;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModalViewModel)));
        }

        public ICommand ShowConfigurationSettings =>
            new DelegateCommand(
                () =>
                {
                    _viewModel = _configurationSettingsViewModel;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewModel"));
                });

        public ICommand ShowDeckManager =>
            new DelegateCommand(
                () =>
                {
                    _viewModel = _deckManagerViewModel;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewModel"));
                });

        public object ViewModel => _viewModel;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

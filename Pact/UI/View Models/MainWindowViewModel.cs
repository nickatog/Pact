using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class MainWindowViewModel
        : INotifyPropertyChanged
    {
        public static MainWindowViewModel Instance { get; private set; }

        private readonly ConfigurationSettingsViewModel _configurationSettingsViewModel;
        private readonly DeckManagerViewModel _deckManagerViewModel;
        private readonly DevToolsViewModel _devToolsViewModel;
        private readonly DownloadUpdatesViewModel _downloadUpdatesViewModel;

        private object _modalViewModel;
        private object _viewModel;

        public MainWindowViewModel(
            ConfigurationSettingsViewModel configurationSettingsViewModel,
            DeckManagerViewModel deckManagerViewModel,
            DevToolsViewModel devToolsViewModel,
            DownloadUpdatesViewModel downloadUpdatesViewModel)
        {
            _configurationSettingsViewModel = configurationSettingsViewModel.Require(nameof(configurationSettingsViewModel));
            _deckManagerViewModel = deckManagerViewModel.Require(nameof(deckManagerViewModel));
            _devToolsViewModel = devToolsViewModel.Require(nameof(devToolsViewModel));
            _downloadUpdatesViewModel = downloadUpdatesViewModel.Require(nameof(downloadUpdatesViewModel));

            _viewModel = _deckManagerViewModel;

            Instance = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public object ModalViewModel
        {
            get => _modalViewModel;
            private set
            {
                _modalViewModel = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModalViewModel)));
            }
        }

        private double _modalViewModelOpacity;
        public double ModalViewModelOpacity
        {
            get => _modalViewModelOpacity;
            set
            {
                _modalViewModelOpacity = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModalViewModelOpacity)));
            }
        }

        public void SetModalViewModel<TResult>(
            IModalViewModel<TResult> viewModel,
            int fadeDuration)
        {
            ModalViewModelOpacity = 1.0d;

            ModalViewModel = viewModel;

            viewModel.OnClosed +=
                __ =>
                {
                    if (fadeDuration <= 0)
                    {
                        ModalViewModel = null;

                        return;
                    }

                    Task.Run(
                        () =>
                        {
                            long fadeTime = Stopwatch.Frequency * fadeDuration / 1000;

                            long startTime = Stopwatch.GetTimestamp();

                            long deltaTime;
                            while ((deltaTime = Stopwatch.GetTimestamp() - startTime) < fadeTime)
                                ModalViewModelOpacity = Math.Log((fadeTime - deltaTime) * 9.0d / fadeTime + 1.0d, 10.0d);

                            ModalViewModel = null;
                        });
                };
        }

        public ICommand ShowConfigurationSettings => new DelegateCommand(() => ViewModel = _configurationSettingsViewModel);

        public ICommand ShowDeckManager => new DelegateCommand(() => ViewModel = _deckManagerViewModel);

        public ICommand ShowDevTools => new DelegateCommand(() => ViewModel = _devToolsViewModel);

        public ICommand ShowDownloadUpdates => new DelegateCommand(() => ViewModel = _downloadUpdatesViewModel);

        public object ViewModel
        {
            get => _viewModel;
            private set
            {
                _viewModel = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewModel)));
            }
        }
    }
}

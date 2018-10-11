#region Namespaces
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
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

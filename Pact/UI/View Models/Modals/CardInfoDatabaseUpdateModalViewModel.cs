using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class CardInfoDatabaseUpdateModalViewModel
        : IModalViewModel<bool>
        , INotifyPropertyChanged
    {
        #region Private members
        private Action _canExecuteDownloadChanged;
        private readonly ICardInfoDatabaseManager _cardInfoDatabaseManager;
        private readonly ICardInfoDatabaseUpdateService _cardInfoDatabaseUpdateService;
        private readonly int? _currentVersion;
        private string _errorMessage;
        private bool _isUpdating;
        private int? _latestVersion;
        private string _latestVersionText;
        private readonly Dispatcher _uiDispatcher;
        #endregion // Private members

        public CardInfoDatabaseUpdateModalViewModel(
        #region Dependency assignments
            ICardInfoDatabaseManager cardInfoDatabaseManager,
            ICardInfoDatabaseUpdateService cardInfoDatabaseUpdateService,
            Dispatcher uiDispatcher)
        {
            _cardInfoDatabaseManager =
                cardInfoDatabaseManager.Require(nameof(cardInfoDatabaseManager));

            _cardInfoDatabaseUpdateService =
                cardInfoDatabaseUpdateService.Require(nameof(cardInfoDatabaseUpdateService));

            _uiDispatcher =
                uiDispatcher.Require(nameof(uiDispatcher));
            #endregion // Dependency assignments

            _currentVersion = _cardInfoDatabaseManager.GetCurrentVersion();

            var cancellation = new CancellationTokenSource();

            Task.Run(
                async () =>
                {
                    int numDots = 1;

                    while (!cancellation.IsCancellationRequested)
                    {
                        LatestVersion = string.Join(" ", Enumerable.Range(1, numDots).Select(__ => "."));

                        await Task.Delay(1000);

                        numDots = (numDots % 3) + 1;
                    }
                });

            Task<int?> getLatestVersion = _cardInfoDatabaseUpdateService.GetLatestVersion();
            getLatestVersion.ContinueWith(__result => cancellation.Cancel());
            getLatestVersion.ContinueWith(
                __result =>
                {
                    _latestVersion = __result.Result;

                    LatestVersion = _latestVersion?.ToString() ?? "unknown";

                    _uiDispatcher.Invoke(() => _canExecuteDownloadChanged?.Invoke());
                },
                TaskContinuationOptions.NotOnFaulted);
            getLatestVersion.ContinueWith(
                __result =>
                {
                    LatestVersion = "unknown";

                    ErrorMessage = "Could not determine latest version!";
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<bool> OnClosed;

        public ICommand Cancel =>
            new DelegateCommand(
                () => OnClosed?.Invoke(false));

        public string CurrentVersion => _currentVersion?.ToString() ?? "unknown";

        public ICommand DownloadLatestCardInfoDatabase =>
            new DelegateCommand(
                async () =>
                {
                    _isUpdating = true;

                    _uiDispatcher.Invoke(() => _canExecuteDownloadChanged?.Invoke());

                    try
                    {
                        using (Stream downloadStream = await _cardInfoDatabaseUpdateService.GetVersionStream(_latestVersion.Value))
                            await _cardInfoDatabaseManager.UpdateCardInfoDatabase(_latestVersion.Value, downloadStream);

                        OnClosed?.Invoke(true);
                    }
                    catch (Exception)
                    {
                        ErrorMessage = "Failed to update card database!";
                    }
                },
                () =>
                    !_isUpdating
                    && _latestVersion.HasValue
                    && (!_currentVersion.HasValue || _latestVersion.Value > _currentVersion.Value),
                __canExecuteChanged => _canExecuteDownloadChanged = __canExecuteChanged);

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
        }

        public string LatestVersion
        {
            get => _latestVersionText;
            set
            {
                _latestVersionText = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LatestVersion)));
            }
        }
    }
}

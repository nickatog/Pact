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
    public sealed class CardDatabaseUpdateModalViewModel
        : IModalViewModel<bool>
        , INotifyPropertyChanged
    {
        #region Private members
        private Action _canExecuteDownloadChanged;
        private readonly ICardDatabaseManager _cardDatabaseManager;
        private readonly ICardDatabaseUpdateService _cardDatabaseUpdateService;
        private readonly int? _currentVersion;
        private string _errorMessage;
        private bool _isUpdating;
        private int? _latestVersion;
        private string _latestVersionText;
        private readonly Dispatcher _uiDispatcher;
        #endregion // Private members

        public CardDatabaseUpdateModalViewModel(
        #region Dependency assignments
            ICardDatabaseManager cardDatabaseManager,
            ICardDatabaseUpdateService cardDatabaseUpdateService,
            Dispatcher uiDispatcher)
        {
            _cardDatabaseManager =
                cardDatabaseManager.Require(nameof(cardDatabaseManager));

            _cardDatabaseUpdateService =
                cardDatabaseUpdateService.Require(nameof(cardDatabaseUpdateService));

            _uiDispatcher =
                uiDispatcher.Require(nameof(uiDispatcher));
            #endregion // Dependency assignments

            _currentVersion = _cardDatabaseManager.GetCurrentVersion();

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

            Task<int?> getLatestVersion = _cardDatabaseUpdateService.GetLatestVersion();
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
                        using (Stream downloadStream = await _cardDatabaseUpdateService.GetVersionStream(_latestVersion.Value))
                            await _cardDatabaseManager.UpdateCardDatabase(_latestVersion.Value, downloadStream);

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

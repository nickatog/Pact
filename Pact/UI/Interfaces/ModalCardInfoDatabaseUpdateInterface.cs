using System.Threading.Tasks;
using System.Windows.Threading;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalCardInfoDatabaseUpdateInterface
        : ICardInfoDatabaseUpdateInterface
    {
        #region Private members
        private readonly ICardInfoDatabaseManager _cardInfoDatabaseManager;
        private readonly ICardInfoDatabaseUpdateService _cardInfoDatabaseUpdateService;
        private readonly IModalDisplay _modalDisplay;
        private readonly Dispatcher _uiDispatcher;
        #endregion // Private members

        public ModalCardInfoDatabaseUpdateInterface(
        #region Dependency assignments
            ICardInfoDatabaseManager cardInfoDatabaseManager,
            ICardInfoDatabaseUpdateService cardInfoDatabaseUpdateService,
            IModalDisplay modalDisplay,
            Dispatcher uiDispatcher)
        {
            _cardInfoDatabaseManager =
                cardInfoDatabaseManager.Require(nameof(cardInfoDatabaseManager));

            _cardInfoDatabaseUpdateService =
                cardInfoDatabaseUpdateService.Require(nameof(cardInfoDatabaseUpdateService));

            _modalDisplay =
                modalDisplay.Require(nameof(modalDisplay));

            _uiDispatcher =
                uiDispatcher.Require(nameof(uiDispatcher));
            #endregion // Dependency assignments
        }

        Task ICardInfoDatabaseUpdateInterface.CheckForUpdates()
        {
            var completionSource = new TaskCompletionSource<bool>();

            _modalDisplay.Show(
                new CardInfoDatabaseUpdateModalViewModel(
                    _cardInfoDatabaseManager,
                    _cardInfoDatabaseUpdateService,
                    _uiDispatcher),
                __ => completionSource.SetResult(true));

            return completionSource.Task;
        }
    }
}

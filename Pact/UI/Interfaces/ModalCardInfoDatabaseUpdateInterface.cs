using System.Threading.Tasks;
using System.Windows.Threading;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalCardDatabaseUpdateInterface
        : ICardDatabaseUpdateInterface
    {
        #region Private members
        private readonly ICardDatabaseManager _cardDatabaseManager;
        private readonly ICardDatabaseUpdateService _cardDatabaseUpdateService;
        private readonly IModalDisplay _modalDisplay;
        private readonly Dispatcher _uiDispatcher;
        #endregion // Private members

        public ModalCardDatabaseUpdateInterface(
        #region Dependency assignments
            ICardDatabaseManager cardDatabaseManager,
            ICardDatabaseUpdateService cardDatabaseUpdateService,
            IModalDisplay modalDisplay,
            Dispatcher uiDispatcher)
        {
            _cardDatabaseManager =
                cardDatabaseManager.Require(nameof(cardDatabaseManager));

            _cardDatabaseUpdateService =
                cardDatabaseUpdateService.Require(nameof(cardDatabaseUpdateService));

            _modalDisplay =
                modalDisplay.Require(nameof(modalDisplay));

            _uiDispatcher =
                uiDispatcher.Require(nameof(uiDispatcher));
            #endregion // Dependency assignments
        }

        Task ICardDatabaseUpdateInterface.CheckForUpdates()
        {
            var completionSource = new TaskCompletionSource<bool>();

            _modalDisplay.Show(
                new CardDatabaseUpdateModalViewModel(
                    _cardDatabaseManager,
                    _cardDatabaseUpdateService,
                    _uiDispatcher),
                __ => completionSource.SetResult(true));

            return completionSource.Task;
        }
    }
}

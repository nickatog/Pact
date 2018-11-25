using System.Threading.Tasks;
using System.Windows.Threading;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalCardDatabaseUpdateInterface
        : ICardDatabaseUpdateInterface
    {
        private readonly ICardDatabaseManager _cardDatabaseManager;
        private readonly ICardDatabaseUpdateService _cardDatabaseUpdateService;
        private readonly IModalDisplay _modalDisplay;
        private readonly Dispatcher _uiDispatcher;

        public ModalCardDatabaseUpdateInterface(
            ICardDatabaseManager cardDatabaseManager,
            ICardDatabaseUpdateService cardDatabaseUpdateService,
            IModalDisplay modalDisplay,
            Dispatcher uiDispatcher)
        {
            _cardDatabaseManager = cardDatabaseManager.Require(nameof(cardDatabaseManager));
            _cardDatabaseUpdateService = cardDatabaseUpdateService.Require(nameof(cardDatabaseUpdateService));
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
            _uiDispatcher = uiDispatcher.Require(nameof(uiDispatcher));
        }

        Task ICardDatabaseUpdateInterface.CheckForUpdates()
        {
            var completionSource = new TaskCompletionSource<object>();

            _modalDisplay.Show(
                new CardDatabaseUpdateModalViewModel(
                    _cardDatabaseManager,
                    _cardDatabaseUpdateService,
                    _uiDispatcher),
                __ => completionSource.SetResult(null));

            return completionSource.Task;
        }
    }
}

using System.IO;
using System.Threading.Tasks;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class EventDispatchingCardDatabaseManager
        : ICardDatabaseManager
    {
        private readonly ICardDatabaseManager _cardDatabaseManager;
        private readonly IEventDispatcher _eventDispatcher;

        public EventDispatchingCardDatabaseManager(
            ICardDatabaseManager cardDatabaseManager,
            IEventDispatcher eventDispatcher)
        {
            _cardDatabaseManager = cardDatabaseManager.Require(nameof(cardDatabaseManager));
            _eventDispatcher = eventDispatcher.Require(nameof(eventDispatcher));
        }

        int? ICardDatabaseManager.GetCurrentVersion()
        {
            return _cardDatabaseManager.GetCurrentVersion();
        }

        async Task ICardDatabaseManager.UpdateCardDatabase(
            int version,
            Stream updateStream)
        {
            await _cardDatabaseManager.UpdateCardDatabase(version, updateStream).ConfigureAwait(false);

            _eventDispatcher.DispatchEvent(new ViewEvents.CardDatabaseUpdated());
        }
    }
}

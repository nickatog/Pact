using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedDeckRepository
        : IDeckRepository
    {
        private readonly AsyncSemaphore _asyncMutex;
        private readonly IDeckFileStorage _deckFileStorage;

        public FileBasedDeckRepository(
            AsyncSemaphore asyncMutex,
            IDeckFileStorage deckFileStorage)
        {
            _asyncMutex = asyncMutex.Require(nameof(asyncMutex));
            _deckFileStorage = deckFileStorage.Require(nameof(deckFileStorage));
        }

        async Task<IEnumerable<Models.Client.Deck>> IDeckRepository.GetAllDecksAndGameResults()
        {
            return
                (await _deckFileStorage.GetAll().ConfigureAwait(false))
                .Select(
                    __deck =>
                        new Models.Client.Deck(
                            __deck.DeckID,
                            __deck.DeckString,
                            __deck.Title,
                            __deck.Position,
                            __deck.GameResults
                            .Select(
                                __gameResult =>
                                    new Models.Client.GameResult(
                                        __gameResult.Timestamp,
                                        __gameResult.GameWon,
                                        __gameResult.OpponentClass))));
        }

        async Task IDeckRepository.ReplaceDecks(
            IEnumerable<Models.Client.DeckDetail> deckDetails)
        {
            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
            {
                await _deckFileStorage.SaveAll(
                    (deckDetails ?? Enumerable.Empty<Models.Client.DeckDetail>())
                    .GroupJoin(
                        (await _deckFileStorage.GetAll().ConfigureAwait(false)),
                        __deckDetails => __deckDetails.DeckID,
                        __deck => __deck.DeckID,
                        (__deckDetails, __decks) => (DeckDetails: __deckDetails, Decks: __decks))
                    .SelectMany(
                        __joinResult => __joinResult.Decks.Cast<Models.Data.Deck?>().DefaultIfEmpty(),
                        (__joinResult, __deck) =>
                            new Models.Data.Deck(
                                __joinResult.DeckDetails.DeckID,
                                __joinResult.DeckDetails.DeckString,
                                __joinResult.DeckDetails.Title,
                                __joinResult.DeckDetails.Position,
                                __deck?.GameResults ?? Enumerable.Empty<Models.Data.GameResult>()))).ConfigureAwait(false);
            }
        }

        async Task IDeckRepository.UpdateDeck(
            Models.Client.DeckDetail deckDetail)
        {
            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
            {
                await _deckFileStorage.SaveAll(
                    (await _deckFileStorage.GetAll().ConfigureAwait(false))
                    .Select(
                        __deck =>
                        {
                            if (__deck.DeckID != deckDetail.DeckID)
                                return __deck;

                            return
                                new Models.Data.Deck(
                                    deckDetail.DeckID,
                                    deckDetail.DeckString,
                                    deckDetail.Title,
                                    deckDetail.Position,
                                    __deck.GameResults);
                        })).ConfigureAwait(false);
            }
        }
    }
}

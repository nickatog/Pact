using System;
using System.Linq;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedGameResultRepository
        : IGameResultRepository
    {
        private readonly AsyncSemaphore _asyncMutex;
        private readonly IDeckFileStorage _deckFileStorage;

        public FileBasedGameResultRepository(
            AsyncSemaphore asyncMutex,
            IDeckFileStorage deckFileStorage)
        {
            _asyncMutex = asyncMutex.Require(nameof(asyncMutex));
            _deckFileStorage = deckFileStorage.Require(nameof(deckFileStorage));
        }

        async Task IGameResultRepository.AddGameResult(
            Guid deckID,
            Models.Client.GameResult gameResult)
        {
            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
            {
                await _deckFileStorage.SaveAll(
                    (await _deckFileStorage.GetAll().ConfigureAwait(false))
                    .Select(
                        __deck =>
                        {
                            if (__deck.DeckID != deckID)
                                return __deck;

                            return
                                new Models.Data.Deck(
                                    __deck.DeckID,
                                    __deck.DeckString,
                                    __deck.Title,
                                    __deck.Position,
                                    __deck.GameResults.Append(
                                        new Models.Data.GameResult(
                                            gameResult.Timestamp,
                                            gameResult.GameWon,
                                            gameResult.OpponentClass)));
                        })).ConfigureAwait(false);
            }
        }
    }
}

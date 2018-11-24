using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedGameResultRepository
        : IGameResultRepository
    {
        private readonly AsyncSemaphore _asyncMutex;
        private readonly IDeckFileStorage _deckInfoFileStorage;

        public FileBasedGameResultRepository(
            AsyncSemaphore asyncMutex,
            IDeckFileStorage deckInfoFileStorage)
        {
            _asyncMutex = asyncMutex.Require(nameof(asyncMutex));
            _deckInfoFileStorage = deckInfoFileStorage.Require(nameof(deckInfoFileStorage));
        }

        async Task IGameResultRepository.AddGameResult(
            Guid deckID,
            Models.Client.GameResult gameResult)
        {
            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
            {
                await _deckInfoFileStorage.SaveAll(
                    (await _deckInfoFileStorage.GetAll().ConfigureAwait(false))
                    .Select(
                        __deckInfo =>
                        {
                            if (__deckInfo.DeckID != deckID)
                                return __deckInfo;

                            return
                                new Models.Data.Deck(
                                    __deckInfo.DeckID,
                                    __deckInfo.DeckString,
                                    __deckInfo.Title,
                                    __deckInfo.Position,
                                    __deckInfo.GameResults
                                        .Concat(
                                    new List<Models.Data.GameResult>()
                                    {
                                        new Models.Data.GameResult(
                                            gameResult.Timestamp,
                                            gameResult.GameWon,
                                            gameResult.OpponentClass)
                                    }));
                        })).ConfigureAwait(false);
            }
        }
    }
}

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
        private readonly IDeckInfoFileStorage _deckInfoFileStorage;

        public FileBasedGameResultRepository(
            AsyncSemaphore asyncMutex,
            IDeckInfoFileStorage deckInfoFileStorage)
        {
            _asyncMutex =
                asyncMutex.Require(nameof(asyncMutex));

            _deckInfoFileStorage =
                deckInfoFileStorage.Require(nameof(deckInfoFileStorage));
        }

        async Task IGameResultRepository.AddGameResult(
            Guid deckID,
            GameResult gameResult)
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
                                new DeckInfo(
                                    __deckInfo.DeckID,
                                    __deckInfo.DeckString,
                                    __deckInfo.Title,
                                    __deckInfo.Position,
                                    __deckInfo.GameResults
                                        .Concat(
                                    new List<GameResult>() { gameResult }));
                        })).ConfigureAwait(false);
            }
        }
    }
}

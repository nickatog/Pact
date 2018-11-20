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
        private readonly IDeckInfoFileStorage _deckInfoFileStorage;

        public FileBasedDeckRepository(
            AsyncSemaphore asyncMutex,
            IDeckInfoFileStorage deckInfoFileStorage)
        {
            _asyncMutex = asyncMutex.Require(nameof(asyncMutex));
            _deckInfoFileStorage = deckInfoFileStorage.Require(nameof(deckInfoFileStorage));
        }

        Task<IEnumerable<DeckInfo>> IDeckRepository.GetAllDecksAndGameResults()
        {
            return _deckInfoFileStorage.GetAll();
        }

        async Task IDeckRepository.ReplaceDecks(
            IEnumerable<DeckDetails> deckDetails)
        {
            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
            {
                await _deckInfoFileStorage.SaveAll(
                    (deckDetails ?? Enumerable.Empty<DeckDetails>())
                    .GroupJoin(
                        (await _deckInfoFileStorage.GetAll().ConfigureAwait(false)),
                        __deckDetails => __deckDetails.DeckID,
                        __deckInfo => __deckInfo.DeckID,
                        (__deckDetails, __deckInfos) => (DeckDetails: __deckDetails, DeckInfos: __deckInfos))
                    .SelectMany(
                        __joinResult => __joinResult.DeckInfos.Cast<DeckInfo?>().DefaultIfEmpty(),
                        (__joinResult, __deckInfo) =>
                        {
                            return
                                new DeckInfo(
                                    __joinResult.DeckDetails.DeckID,
                                    __joinResult.DeckDetails.DeckString,
                                    __joinResult.DeckDetails.Title,
                                    __joinResult.DeckDetails.Position,
                                    __deckInfo?.GameResults ?? Enumerable.Empty<GameResult>());
                        })).ConfigureAwait(false);
            }
        }

        async Task IDeckRepository.UpdateDeck(
            DeckDetails deckDetails)
        {
            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
            {
                await _deckInfoFileStorage.SaveAll(
                    (await _deckInfoFileStorage.GetAll().ConfigureAwait(false))
                    .Select(
                        __deckInfo =>
                        {
                            if (__deckInfo.DeckID != deckDetails.DeckID)
                                return __deckInfo;

                            return
                                new DeckInfo(
                                    __deckInfo.DeckID,
                                    deckDetails.DeckString,
                                    deckDetails.Title,
                                    deckDetails.Position,
                                    __deckInfo.GameResults);
                        })).ConfigureAwait(false);
            }
        }
    }
}

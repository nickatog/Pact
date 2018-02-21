using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedGameResultStorage
        : IGameResultStorage
    {
        private readonly ICollectionSerializer<DeckInfo> _deckInfoCollectionSerializer;

        private readonly string _filePath;

        public FileBasedGameResultStorage(
            ICollectionSerializer<DeckInfo> deckInfoCollectionSerializer,
            string filePath)
        {
            _deckInfoCollectionSerializer = deckInfoCollectionSerializer.ThrowIfNull(nameof(deckInfoCollectionSerializer));

            _filePath = filePath.ThrowIfNull(nameof(filePath));
        }

        async Task IGameResultStorage.SaveGameResult(Guid deckID, GameResult gameResult)
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists || fileInfo.Length <= 0)
                return;

            IList<DeckInfo> deckInfos;
            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate))
                deckInfos = new List<DeckInfo>(await _deckInfoCollectionSerializer.Deserialize(stream));

            DeckInfo deckInfo = deckInfos.First(__deck => __deck.DeckID == deckID);
            var gameResults = new List<GameResult>(deckInfo.GameResults) { gameResult };

            int deckIndex = deckInfos.IndexOf(deckInfo);
            deckInfos.RemoveAt(deckIndex);
            deckInfos.Insert(deckIndex, new DeckInfo(deckInfo.DeckID, deckInfo.DeckString, deckInfo.Title, deckInfo.Position, gameResults));
            
            using (var stream = new FileStream(_filePath, FileMode.Create))
                await _deckInfoCollectionSerializer.Serialize(stream, deckInfos);
        }
    }
}

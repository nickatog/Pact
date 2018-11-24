using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckFileStorage
        : IDeckFileStorage
    {
        private readonly ICollectionSerializer<Models.Data.Deck> _deckCollectionSerializer;
        private readonly string _filePath;

        public DeckFileStorage(
            ICollectionSerializer<Models.Data.Deck> deckCollectionSerializer,
            string filePath)
        {
            _deckCollectionSerializer = deckCollectionSerializer.Require(nameof(deckCollectionSerializer));
            _filePath = filePath.Require(nameof(filePath));
        }

        async Task<IEnumerable<Models.Data.Deck>> IDeckFileStorage.GetAll()
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists || fileInfo.Length <= 0)
                return Enumerable.Empty<Models.Data.Deck>();
            
            using (var stream = new FileStream(_filePath, FileMode.Open))
                return await _deckCollectionSerializer.Deserialize(stream).ConfigureAwait(false);
        }

        Task IDeckFileStorage.SaveAll(
            IEnumerable<Models.Data.Deck> decks)
        {
            decks.Require(nameof(decks));

            return __SaveAll();

            async Task __SaveAll()
            {
                var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(_filePath));
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                using (var stream = new FileStream(_filePath, FileMode.Create))
                    await _deckCollectionSerializer.Serialize(stream, decks).ConfigureAwait(false);
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckInfoFileStorage
        : IDeckInfoFileStorage
    {
        private readonly ICollectionSerializer<DeckInfo> _deckInfoCollectionSerializer;
        private readonly string _filePath;

        public DeckInfoFileStorage(
            ICollectionSerializer<DeckInfo> deckInfoCollectionSerializer,
            string filePath)
        {
            _deckInfoCollectionSerializer = deckInfoCollectionSerializer.Require(nameof(deckInfoCollectionSerializer));
            _filePath = filePath.Require(nameof(filePath));
        }

        async Task<IEnumerable<DeckInfo>> IDeckInfoFileStorage.GetAll()
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists || fileInfo.Length <= 0)
                return Enumerable.Empty<DeckInfo>();
            
            using (var stream = new FileStream(_filePath, FileMode.Open))
                return await _deckInfoCollectionSerializer.Deserialize(stream).ConfigureAwait(false);
        }

        Task IDeckInfoFileStorage.SaveAll(
            IEnumerable<DeckInfo> deckInfos)
        {
            deckInfos.Require(nameof(deckInfos));

            return __SaveAll();

            async Task __SaveAll()
            {
                var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(_filePath));
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                using (var stream = new FileStream(_filePath, FileMode.Create))
                    await _deckInfoCollectionSerializer.Serialize(stream, deckInfos).ConfigureAwait(false);
            }
        }
    }
}

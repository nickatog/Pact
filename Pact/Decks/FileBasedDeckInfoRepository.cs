using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedDeckInfoRepository
        : IDeckInfoRepository
    {
        private readonly ICollectionSerializer<DeckInfo> _deckInfoCollectionSerializer;

        private readonly string _filePath;

        public FileBasedDeckInfoRepository(
            ICollectionSerializer<DeckInfo> deckInfoCollectionSerializer,
            string filePath)
        {
            _deckInfoCollectionSerializer = deckInfoCollectionSerializer.Require(nameof(deckInfoCollectionSerializer));

            _filePath = filePath.Require(nameof(filePath));
        }

        async Task<IEnumerable<DeckInfo>> IDeckInfoRepository.GetAll()
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists || fileInfo.Length <= 0)
                return Enumerable.Empty<DeckInfo>();

            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate))
                return await _deckInfoCollectionSerializer.Deserialize(stream).ConfigureAwait(false);
        }

        async Task IDeckInfoRepository.Save(IEnumerable<DeckInfo> deckInfos)
        {
            var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(_filePath));
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            using (var stream = new FileStream(_filePath, FileMode.Create))
                await _deckInfoCollectionSerializer.Serialize(stream, deckInfos).ConfigureAwait(false);
        }
    }
}

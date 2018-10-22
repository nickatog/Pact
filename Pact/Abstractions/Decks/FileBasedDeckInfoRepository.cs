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

        Task<IEnumerable<DeckInfo>> IDeckInfoRepository.GetAll()
        {
            return _GetAll();
        }

        async Task IDeckInfoRepository.Save(DeckInfo deckInfo)
        {
            IEnumerable<DeckInfo> deckInfos =
                (await _GetAll().ConfigureAwait(false))
                .Where(__deckInfo => __deckInfo.DeckID != deckInfo.DeckID)
                .Concat(new List<DeckInfo> { deckInfo });

            await _SaveAll(deckInfos).ConfigureAwait(false);
        }

        Task IDeckInfoRepository.ReplaceAll(IEnumerable<DeckInfo> deckInfos)
        {
            return _SaveAll(deckInfos);
        }

        private async Task<IEnumerable<DeckInfo>> _GetAll()
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists || fileInfo.Length <= 0)
                return Enumerable.Empty<DeckInfo>();

            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate))
                return await _deckInfoCollectionSerializer.Deserialize(stream).ConfigureAwait(false);
        }

        private async Task _SaveAll(IEnumerable<DeckInfo> deckInfos)
        {
            var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(_filePath));
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            using (var stream = new FileStream(_filePath, FileMode.Create))
                await _deckInfoCollectionSerializer.Serialize(stream, deckInfos).ConfigureAwait(false);
        }
    }
}

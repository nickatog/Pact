using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckInfoRepository
    {
        IEnumerable<DeckInfo> GetAll();

        void Save(
            IEnumerable<DeckInfo> deckInfos);
    }

    public sealed class DeckInfoRepository
        : IDeckInfoRepository
    {
        private readonly string _filePath = @"C:\Users\Nicholas Anderson\Documents\Visual Studio 2017\Projects\Pact\decks.dat";

        IEnumerable<DeckInfo> IDeckInfoRepository.GetAll()
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
                return Enumerable.Empty<DeckInfo>();

            var serializer = new BinaryFormatter();

            IEnumerable<DeckInfo> stuff;

            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate))
                stuff = (IEnumerable<DeckInfo>)serializer.Deserialize(stream);

            return stuff;
        }

        void IDeckInfoRepository.Save(IEnumerable<DeckInfo> deckInfos)
        {
            var serializer = new BinaryFormatter();

            using (var stream = new FileStream(_filePath, FileMode.Create))
                serializer.Serialize(stream, deckInfos.ToList());
        }
    }

    [Serializable]
    public struct DeckInfo
    {
        public Guid DeckID { get; private set; }

        public string DeckString { get; set; }

        private List<GameResult> _gameResults;
        public IEnumerable<GameResult> GameResults => _gameResults;

        public DeckInfo(
            Guid deckID,
            string deckString,
            IEnumerable<GameResult> gameResults)
        {
            DeckID = deckID;
            DeckString = deckString;
            _gameResults = new List<GameResult>(gameResults);
        }
    }
}

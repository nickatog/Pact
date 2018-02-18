using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Pact
{
    [Serializable]
    public struct DeckInfo
        : ISerializable<DeckInfo>
    {
        private readonly Guid _deckID;
        private readonly string _deckString;
        private readonly IList<GameResult> _gameResults;
        private readonly UInt16 _position;

        public DeckInfo(
            Guid deckID,
            string deckString,
            UInt16 position,
            IEnumerable<GameResult> gameResults)
        {
            _deckID = deckID;
            _deckString = deckString;
            _gameResults = new List<GameResult>(gameResults);
            _position = position;
        }

        public Guid DeckID => _deckID;

        public string DeckString => _deckString;

        public IEnumerable<GameResult> GameResults => _gameResults;

        public UInt16 Position => _position;

        public static Task<DeckInfo> Deserialize(Stream stream)
        {
            var serializer = new BinaryFormatter();

            return Task.FromResult((DeckInfo)serializer.Deserialize(stream));
        }

        public Task Serialize(Stream stream)
        {
            var serializer = new BinaryFormatter();

            serializer.Serialize(stream, this);

            return Task.CompletedTask;
        }
    }
}

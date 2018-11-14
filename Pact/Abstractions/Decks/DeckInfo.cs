using System;
using System.Collections.Generic;

namespace Pact
{
    [Serializable]
    public struct DeckInfo
    {
        private readonly Guid _deckID;
        private readonly string _deckString;
        private readonly IList<GameResult> _gameResults;
        private readonly UInt16 _position;
        private readonly string _title;

        public DeckInfo(
            Guid deckID,
            string deckString,
            string title,
            UInt16 position,
            IEnumerable<GameResult> gameResults)
        {
            _deckID = deckID;
            _deckString = deckString;
            _gameResults = new List<GameResult>(gameResults);
            _position = position;
            _title = title;
        }

        public Guid DeckID => _deckID;

        public string DeckString => _deckString;

        public IEnumerable<GameResult> GameResults => _gameResults;

        public UInt16 Position => _position;

        public string Title => _title;
    }
}

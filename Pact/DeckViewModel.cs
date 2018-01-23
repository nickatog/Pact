using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Pact
{
    public sealed class DeckViewModel
        : INotifyPropertyChanged
    {
        private readonly Guid _deckID;
        private readonly Decklist _decklist;
        private IList<GameResult> _gameResults;
        private readonly IGameResultStorage _gameResultStorage;
        private readonly Action<Decklist, Action> _trackDeckHandler;
        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly ICardInfoProvider _cardInfoProvider;

        public string DeckString { get; private set; }
        public IEnumerable<GameResult> GameResults => _gameResults;
        public Guid DeckID => _deckID;
        public string Class { get; private set; }

        public DeckViewModel(
            Guid deckID,
            string deckString,
            Decklist decklist,
            Action<Decklist, Action> trackDeckHandler,
            IGameResultStorage gameResultStorage,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            string @class,
            IConfigurationSettings configurationSettings,
            ICardInfoProvider cardInfoProvider,
            IEnumerable<GameResult> gameResults = null)
        {
            _trackDeckHandler =
                trackDeckHandler
                ?? throw new ArgumentNullException(nameof(trackDeckHandler));

            _gameResultStorage =
                gameResultStorage
                ?? throw new ArgumentNullException(nameof(gameResultStorage));

            _gameEventDispatcher =
                gameEventDispatcher
                ?? throw new ArgumentNullException(nameof(gameEventDispatcher));

            _configurationSettings =
                configurationSettings
                ?? throw new ArgumentNullException(nameof(configurationSettings));

            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            Class = @class;
            _deckID = deckID;
            _decklist = decklist;
            DeckString = deckString;

            _gameResults = new List<GameResult>(gameResults ?? Enumerable.Empty<GameResult>());

            ___asdf =
                new Valkyrie.DelegateEventHandler<Events.GameEnded>(
                    __event =>
                    {
                        var timestamp = DateTime.UtcNow;

                        bool gameWon = __event.Winners.Contains(_configurationSettings.AccountName);

                        string opponentHeroID = __event.HeroCardIDs.FirstOrDefault(__heroID => __heroID != decklist.HeroID);
                        opponentHeroID = opponentHeroID ?? decklist.HeroID;
                        string opponentClass = _cardInfoProvider.GetCardInfo(opponentHeroID)?.Class;

                        var gameResult = new GameResult(timestamp, gameWon, opponentClass);

                        _gameResults.Add(gameResult);

                        RefreshStats();

                        _gameResultStorage.SaveGameResult(_deckID, gameResult);
                    });
        }

        private readonly Valkyrie.IEventHandler ___asdf;

        public ICommand TrackDeck =>
            new DelegateCommand(
                () =>
                {
                    _trackDeckHandler(_decklist, () => _gameEventDispatcher.UnregisterHandler(___asdf));

                    // need some way of unregistering this handler when other decks are selected to track
                    // pass delegate in constructor to pass unregistration for later
                    // another delegate to call when tracking results that will call the unregister function?

                    _gameEventDispatcher.RegisterHandler(___asdf);
                });

        private void RefreshStats()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Wins"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Losses"));
        }

        public int Wins => _gameResults.Count(__gameResult => __gameResult.GameWon);
        public int Losses => _gameResults.Count(__gameResult => !__gameResult.GameWon);

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [Serializable]
    public struct GameResult
    {
        // win/loss
        // coin status
        // opponent class/deck?
        // timestamp?
        // duration?

        public bool GameWon { get; private set; }
        public string OpponentClass { get; private set; }
        public DateTime Timestamp { get; private set; }

        public GameResult(
            DateTime timestamp,
            bool gameWon,
            string opponentClass)
        {
            GameWon = gameWon;
            OpponentClass = opponentClass;
            Timestamp = timestamp;
        }
    }
}

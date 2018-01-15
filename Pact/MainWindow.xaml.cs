using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Pact
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IEventStream _eventStream;

        public MainWindow()
        {
            InitializeComponent();

            var eventParsers =
                new List<IGameStateDebugEventParser>
                {
                    new EventParsers.PowerLog.GameStateDebug.Block(),
                    new EventParsers.PowerLog.GameStateDebug.CreateGame(),
                    new EventParsers.PowerLog.GameStateDebug.FullEntity(),
                    new EventParsers.PowerLog.GameStateDebug.HideEntity(),
                    new EventParsers.PowerLog.GameStateDebug.ShowEntity(),
                    new EventParsers.PowerLog.GameStateDebug.TagChange()
                };
            // @"C:\Program Files (x86)\Hearthstone\Logs\Power.log",
            // @"C:\Users\Nicholas Anderson\Desktop\Power2.log",
            _eventStream =
                new PowerLogEventStream(
                    @"C:\Program Files (x86)\Hearthstone\Logs\Power.log",
                    new GameStateDebugPowerLogEventParser(eventParsers));

            ICardInfoProvider cardInfoProvider =
                new JSONCardInfoProvider(@"C:\Users\Nicholas Anderson\Documents\Visual Studio 2017\Projects\Pact\cards.json");

            Valkyrie.IEventDispatcherFactory eventDispatcherFactory = new Valkyrie.InMemoryEventDispatcherFactory();
            Valkyrie.IEventDispatcher eventDispatcher = eventDispatcherFactory.Create();

            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.CardDrawnFromDeck>(
                    __event => System.Diagnostics.Debug.WriteLine($"{DateTime.Now} - Player card draw: {cardInfoProvider.GetCardInfo(__event.CardID).Value.Name}")));

            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.CardEnteredPlayFromDeck>(
                    __event => System.Diagnostics.Debug.WriteLine($"{DateTime.Now} - Card entered play from deck: {cardInfoProvider.GetCardInfo(__event.CardID).Value.Name}")));

            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.GameEnded>(
                    __event =>
                        System.Diagnostics.Debug.WriteLine(
                            $"{DateTime.Now} - Game ended: {string.Join(", ", __event.Winners)} beat {string.Join(", ", __event.Losers)}!")));

            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.OpponentCoinLost>(
                    __event => System.Diagnostics.Debug.WriteLine($"{DateTime.Now} - Opponent no longer has the coin!")));

            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.OpponentReceivedCoin>(
                    __event => System.Diagnostics.Debug.WriteLine($"{DateTime.Now} - Opponent received the coin!")));

            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.PlayerReceivedCoin>(
                    __event => System.Diagnostics.Debug.WriteLine($"{DateTime.Now} - Player received the coin!")));

            IDeckStringSerializer s = new DeckStringSerializer(cardInfoProvider);

            // mill rogue: AAECAaIHCLICqAipzQKxzgKA0wLQ4wLf4wK77wILigG0AcQB7QLLA80D+AeGCamvAuXRAtvjAgA=
            // zoo: AAECAcn1AgTECJG8ApfTApziAg0w9wSoBc4H5QfCCLy2AsrDApvLAvfNAqbOAvLQAvvTAgA=

            var tracker =
                new PlayerDeckTrackerView(
                    new PlayerDeckTrackerViewModel(
                        s.Deserialize("AAECAcn1AgTECJG8ApfTApziAg0w9wSoBc4H5QfCCLy2AsrDApvLAvfNAqbOAvLQAvvTAgA="),
                        eventDispatcher,
                        cardInfoProvider));

            tracker.Show();

            Task.Run(
                async () =>
                {
                    while (true)
                    {
                        try
                        {
                            object @event = await _eventStream.ReadNext();

                            eventDispatcher.DispatchEvent(@event);
                        } catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                        }
                    }
                });
        }
    }

    public interface IDeckStringSerializer
    {
        Decklist Deserialize(
            string text);
    }

    public sealed class DeckStringSerializer
        : IDeckStringSerializer
    {
        private readonly ICardInfoProvider _cardInfoProvider;

        public DeckStringSerializer(
            ICardInfoProvider cardInfoProvider)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));
        }

        Decklist IDeckStringSerializer.Deserialize(
            string text)
        {
            using (var stream = new System.IO.MemoryStream(Convert.FromBase64String(text)))
            {
                stream.Seek(3, System.IO.SeekOrigin.Begin);

                int count = ParseVarint(stream);
                var heroes = new int[count];
                for (int counter = 0; counter < count; counter++)
                    heroes[counter] = ParseVarint(stream);

                count = ParseVarint(stream);
                var singleCards = new int[count];
                for (int counter = 0; counter < count; counter++)
                    singleCards[counter] = ParseVarint(stream);

                count = ParseVarint(stream);
                var doubleCards = new int[count];
                for (int counter = 0; counter < count; counter++)
                    doubleCards[counter] = ParseVarint(stream);

                count = ParseVarint(stream);
                var variableCards = new (int, int)[count];
                for (int counter = 0; counter < count; counter++)
                    variableCards[counter] = (ParseVarint(stream), ParseVarint(stream));

                return
                    new Decklist(
                        _cardInfoProvider.GetCardInfo(heroes.FirstOrDefault())?.ID,
                        singleCards.Select(__databaseID => (_cardInfoProvider.GetCardInfo(__databaseID)?.ID, 1))
                        .Concat(doubleCards.Select(__databaseID => (_cardInfoProvider.GetCardInfo(__databaseID)?.ID, 2)))
                        .ToList());
            }

            int ParseVarint(System.IO.Stream stream)
            {
                int result = 0;

                int bytesRead = 0;

                int byteValue;
                while ((byteValue = stream.ReadByte()) != -1)
                {
                    int shiftedValue = (byteValue & 0x7F) << bytesRead * 7;

                    result += shiftedValue;

                    bytesRead++;

                    if ((byteValue & 0x80) == 0)
                        break;
                }

                return result;
            }
        }
    }
}

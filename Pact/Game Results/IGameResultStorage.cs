using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Pact
{
    public interface IGameResultStorage
    {
        Task SaveGameResult(
            Guid deckID,
            GameResult gameResult);
    }

    public sealed class GameResultStorage
        : IGameResultStorage
    {
        private readonly string _filePath = @"C:\Users\Nicholas Anderson\Documents\Visual Studio 2017\Projects\Pact\decks.dat";

        Task IGameResultStorage.SaveGameResult(Guid deckID, GameResult gameResult)
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
                return Task.CompletedTask;

            var serializer = new BinaryFormatter();

            IList<DeckInfo> stuff;

            using (var stream = new FileStream(_filePath, FileMode.Open))
                stuff = (IList<DeckInfo>)serializer.Deserialize(stream);

            DeckInfo deck = stuff.First(__deck => __deck.DeckID == deckID);
            IList<GameResult> gameResults = new List<GameResult>(deck.GameResults) { gameResult };

            int index = stuff.IndexOf(deck);
            stuff.RemoveAt(index);

            stuff.Insert(index, new DeckInfo(deck.DeckID, deck.DeckString, gameResults));

            using (var stream = new FileStream(_filePath, FileMode.Create))
                serializer.Serialize(stream, stuff);

            return Task.CompletedTask;
        }
    }
}

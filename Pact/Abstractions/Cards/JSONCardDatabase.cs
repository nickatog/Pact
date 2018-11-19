using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class JSONCardDatabase
        : ICardDatabase
    {
        private readonly string _filePath;

        public JSONCardDatabase(
            string filePath)
        {
            _filePath = filePath.Require(nameof(filePath));
        }

        Task<IEnumerable<CardInfo>> ICardDatabase.GetCards()
        {
            return
                Task.FromResult<IEnumerable<CardInfo>>(
                    JsonConvert.DeserializeObject<Card[]>(File.ReadAllText(_filePath))
                    .Select(
                        __card =>
                            new CardInfo(
                                __card.name,
                                __card.cardClass,
                                __card.cost,
                                __card.id,
                                __card.dbfId))
                    .ToList());
        }

        private struct Card
        {
            public string cardClass;
            public int cost;
            public int dbfId;
            public string id;
            public string name;
        }
    }
}

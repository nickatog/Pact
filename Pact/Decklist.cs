using System.Collections.Generic;
using System.Linq;

namespace Pact
{
    public struct Decklist
    {
        public IEnumerable<(string CardID, int Count)> Cards { get; private set; }
        public string HeroID { get; private set; }

        public Decklist(
            string heroID,
            IEnumerable<(string, int)> cards)
        {
            Cards = cards ?? Enumerable.Empty<(string, int)>();
            HeroID = heroID;
        }
    }
}

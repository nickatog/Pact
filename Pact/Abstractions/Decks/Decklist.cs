using System.Collections.Generic;
using System.Linq;

namespace Pact
{
    public struct Decklist
    {
        public Decklist(
            string heroID,
            IEnumerable<DecklistCard> cards)
        {
            HeroID = heroID;
            Cards = cards?.ToList() ?? Enumerable.Empty<DecklistCard>();
        }

        public IEnumerable<DecklistCard> Cards { get; }

        public string HeroID { get; }
    }
}

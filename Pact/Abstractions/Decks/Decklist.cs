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
            Cards = cards?.ToList() ?? Enumerable.Empty<DecklistCard>();
            HeroID = heroID;
        }

        public IEnumerable<DecklistCard> Cards { get; }

        public string HeroID { get; }
    }
}

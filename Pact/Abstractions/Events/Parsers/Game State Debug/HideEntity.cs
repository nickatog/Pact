using System.Collections.Generic;
using System.Linq;

using Pact.Extensions.String;

namespace Pact.GameStateDebugEventParsers
{
    public sealed class HideEntity
        : GameStateDebugEventParser
    {
        public HideEntity()
            : base("HIDE_ENTITY - .*$") {}

        protected override IEnumerable<string> ParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            ParserContext parserContext,
            ref IEnumerable<object> parsedEvents)
        {
            IDictionary<string, string> attributes = parserContext.StartLine.ParseKeyValuePairs();
            attributes.TryGetValue("tag", out string tag);
            attributes.TryGetValue("value", out string value);

            attributes.TryGetValue("Entity", out string entity);
            IDictionary<string, string> entityAttributes = entity.Rem("[", "]").ParseKeyValuePairs();
            entityAttributes.TryGetValue("zone", out string zone);
            entityAttributes.TryGetValue("cardId", out string cardID);
            entityAttributes.TryGetValue("player", out string player);

            int.TryParse(player, out int playerID);

            var events = new List<object>();

            if (tag.Eq("ZONE") && value.Eq("DECK") && zone.Eq("HAND"))
            {
                events.Add(new GameEvents.CardReturnedToDeckFromHand(playerID, cardID));
            }

            parsedEvents = events;

            return Enumerable.Empty<string>();
        }
    }
}

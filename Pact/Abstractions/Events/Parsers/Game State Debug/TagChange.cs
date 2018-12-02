using System.Collections.Generic;
using System.Linq;

using Pact.Extensions.String;

namespace Pact.GameStateDebugEventParsers
{
    public sealed class TagChange
        : GameStateDebugEventParser
    {
        public TagChange()
            : base("TAG_CHANGE .*$") {}

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
            entityAttributes.TryGetValue("id", out string entityID);
            entityAttributes.TryGetValue("zone", out string entityZone);

            entityAttributes.TryGetValue("player", out string entityPlayer);
            int.TryParse(entityPlayer, out int playerID);

            var events = new List<object>();

            if (tag.Eq("STEP"))
                parseContext.CurrentGameStep = value;
            else if (tag.Eq("PLAYSTATE"))
            {
                if (value.Eq("WON"))
                    parseContext.GameWinners.Add(entity);
                else if (value.Eq("LOST"))
                    parseContext.GameLosers.Add(entity);
            }
            else if (tag.Eq("ZONE") && entityZone.Eq("DECK") && value.Eq("PLAY") && parseContext.EntityMappings.TryGetValue(entityID, out string cardID))
                events.Add(new GameEvents.CardEnteredPlayFromDeck(playerID, cardID));
            else if (tag.Eq("ZONE") && entityZone.Eq("PLAY") && value.Eq("DECK") && entityAttributes.TryGetValue("cardId", out string entityCardID))
                events.Add(new GameEvents.CardAddedToDeck(playerID, entityCardID));
            else if (tag.Eq("ZONE") && value.Eq("GRAVEYARD") && parseContext.CoinEntityID != null && parseContext.CoinEntityID.Eq(entityID))
                events.Add(new GameEvents.OpponentCoinLost());
            else if (tag.Eq("STATE") && value.Eq("COMPLETE"))
            {
                if (parseContext.PlayerID != null && parseContext.PlayerNames.TryGetValue(parseContext.PlayerID, out string playerName))
                {
                    bool gameWon = parseContext.GameWinners.Contains(playerName);

                    string opponentHeroCardID =
                        parseContext.PlayerHeroCards
                        .Where(heroCardIDByPlayerName => heroCardIDByPlayerName.Key != playerName)
                        .Select(heroCardIDByPlayerName => heroCardIDByPlayerName.Value)
                        .FirstOrDefault();

                    events.Add(new GameEvents.GameEnded(gameWon, opponentHeroCardID));
                }
            }

            parsedEvents = events;

            return Enumerable.Empty<string>();
        }
    }
}

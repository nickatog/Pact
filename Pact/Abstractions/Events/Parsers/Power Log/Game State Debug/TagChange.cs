﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Pact.Extensions.String;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class TagChange
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_tagChangePattern =
            new Regex(@"^\s*TAG_CHANGE .*$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            if (!s_tagChangePattern.IsMatch(currentLine))
                return null;

            IDictionary<string, string> attributes = currentLine.ParseKeyValuePairs();
            attributes.TryGetValue("Entity", out string entity);
            attributes.TryGetValue("tag", out string tag);
            attributes.TryGetValue("value", out string value);

            IDictionary<string, string> entityAttributes = entity.Replace("[", string.Empty).Replace("]", string.Empty).ParseKeyValuePairs();
            entityAttributes.TryGetValue("id", out string entityID);
            entityAttributes.TryGetValue("zone", out string entityZone);
            entityAttributes.TryGetValue("player", out string entityPlayer);
            int.TryParse(entityPlayer, out int playerID);

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            if (tag.Eq("STEP"))
            {
                parseContext.CurrentGameStep = value;
            }
            else if (tag.Eq("PLAYSTATE"))
            {
                if (value.Eq("WON"))
                    parseContext.GameWinners.Add(entity);
                else if (value.Eq("LOST"))
                    parseContext.GameLosers.Add(entity);
            }
            else if (tag.Eq("ZONE") && entityZone.Eq("DECK") && value.Eq("PLAY") && parseContext.EntityMappings.TryGetValue(entityID, out string cardID))
            {
                events.Add(new GameEvents.CardEnteredPlayFromDeck(playerID, cardID));
            }
            else if (tag.Eq("ZONE") && entityZone.Eq("PLAY") && value.Eq("DECK") && entityAttributes.TryGetValue("cardId", out string entityCardID))
            {
                events.Add(new GameEvents.CardAddedToDeck(playerID, entityCardID));
            }
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

            return linesConsumed;
        }
    }
}

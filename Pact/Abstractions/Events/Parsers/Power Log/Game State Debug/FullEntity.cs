using System.Collections.Generic;
using System.Text.RegularExpressions;

using Pact.Extensions.String;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class FullEntity
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_fullEntityPattern =
            new Regex(@"^(?<Offset>\s*)FULL_ENTITY - Creating (?<Attributes>.*)$", RegexOptions.Compiled);

        private static readonly Regex s_tagPattern =
            new Regex(@"^\s*tag=(?<tag>\S*) value=(?<value>.*)$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            Match match = s_fullEntityPattern.Match(currentLine);
            if (!match.Success)
                return null;

            var nestedOffsetPattern = new Regex($@"^{match.Groups["Offset"].Value}    .*$");

            IDictionary<string, string> attributes = match.Groups["Attributes"].Value.ParseKeyValuePairs();
            attributes.TryGetValue("CardID", out string cardID);
            attributes.TryGetValue("ID", out string id);

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            var tags = new Dictionary<string, string>();
            Match tagMatch;

            while ((currentLine = lines.Current) != null)
            {
                if (!nestedOffsetPattern.IsMatch(currentLine))
                {
                    tags.TryGetValue("CARDTYPE", out string cardTypeTag);
                    tags.TryGetValue("CONTROLLER", out string controllerTag);
                    tags.TryGetValue("ZONE", out string zoneTag);
                    int.TryParse(controllerTag, out int playerID);

                    if (parseContext.ParentBlock != null)
                    {
                        parseContext.ParentBlock.Attributes.TryGetValue("BlockType", out string parentBlockType);
                        parseContext.ParentBlock.Attributes.TryGetValue("Entity", out string parentBlockEntity);

                        IDictionary<string, string> parentBlockEntityAttributes = parentBlockEntity.ParseKeyValuePairs();
                        parentBlockEntityAttributes.TryGetValue("cardId", out string parentBlockEntityCardID);

                        if (parentBlockType.Eq("POWER") && zoneTag.Eq("DECK"))
                        {
                            // Fal'dorei Strider
                            if (parentBlockEntityCardID.Eq("LOOT_026"))
                                events.Add(new GameEvents.CardAddedToDeck(playerID, "LOOT_026e"));
                            // Jade Idol
                            else if (parentBlockEntityCardID.Eq("CFM_602"))
                                events.Add(new GameEvents.CardAddedToDeck(playerID, "CFM_602"));
                            // Un'Goro Pack
                            else if (parentBlockEntityCardID.Eq("UNG_851"))
                                events.Add(new GameEvents.CardAddedToDeck(playerID, "UNG_851t1"));
                        }
                        else if (parentBlockType.Eq("TRIGGER") && zoneTag.Eq("DECK"))
                        {
                            // Augmented Elekk
                            if (parentBlockEntityCardID.Eq("BOT_559"))
                            {
                                // TODO:
                                // The log file itself doesn't indicate what card was added to the deck
                                // Keep track of most recent card(s) added to deck and replay the events?
                                // Track cards added during the current main block and clear on new main block start
                            }
                            // Kingsbane
                            if (parentBlockEntityCardID.Eq("LOOT_542"))
                                events.Add(new GameEvents.CardAddedToDeck(playerID, "LOOT_542"));
                        }
                    }

                    if (cardTypeTag.Eq("HERO") && !parseContext.PlayerHeroCards.ContainsKey(controllerTag))
                        parseContext.PlayerHeroCards[controllerTag] = cardID;
                    
                    if (parseContext.CurrentGameStep == null)
                    {
                        if (cardID.Eq("GAME_005"))
                            events.Add(new GameEvents.PlayerReceivedCoin());
                        else if (cardID == string.Empty && zoneTag.Eq("HAND"))
                        {
                            parseContext.CoinEntityID = id;

                            events.Add(new GameEvents.OpponentReceivedCoin());
                        }
                    }

                    parsedEvents = events;

                    return linesConsumed;
                }

                if ((tagMatch = s_tagPattern.Match(currentLine)).Success)
                    tags.Add(tagMatch.Groups["tag"].Value, tagMatch.Groups["value"].Value);

                linesConsumed.Add(currentLine);
                lines.MoveNext();
            }

            return linesConsumed;
        }
    }
}

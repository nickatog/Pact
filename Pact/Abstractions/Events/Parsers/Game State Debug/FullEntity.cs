using System.Collections.Generic;
using System.Text.RegularExpressions;

using Pact.Extensions.String;

namespace Pact.GameStateDebugEventParsers
{
    public sealed class FullEntity
        : GameStateDebugEventParser
    {
        private static readonly Regex s_tagPattern =
            new Regex(@"^\s*tag=(?<tag>\S*) value=(?<value>.*)$", RegexOptions.Compiled);

        public FullEntity()
            : base("FULL_ENTITY - Creating (?<Attributes>.*)$") {}

        protected override IEnumerable<string> ParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            ParserContext parserContext,
            ref IEnumerable<object> parsedEvents)
        {
            IDictionary<string, string> attributes = parserContext.StartGroupValues["Attributes"].ParseKeyValuePairs();
            attributes.TryGetValue("CardID", out string cardID);
            attributes.TryGetValue("ID", out string id);

            var events = new List<object>();
            var linesConsumed = new List<string>();
            var tags = new Dictionary<string, string>();

            while (!lines.HasCompleted)
            {
                string currentLine = lines.Current;

                // Check if the current line is outside the scope of this parser
                if (!parserContext.NestedOffsetPattern.IsMatch(currentLine))
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
                            // Direhorn Hatchling
                            else if (parentBlockEntityCardID.Eq("UNG_957"))
                                events.Add(new GameEvents.CardAddedToDeck(playerID, "UNG_957t1"));
                            // Kingsbane
                            else if (parentBlockEntityCardID.Eq("LOOT_542"))
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

                Match tagMatch;
                if ((tagMatch = s_tagPattern.Match(currentLine)).Success)
                    tags.Add(tagMatch.Groups["tag"].Value, tagMatch.Groups["value"].Value);

                linesConsumed.Add(currentLine);
                lines.MoveNext();
            }

            return linesConsumed;
        }
    }
}

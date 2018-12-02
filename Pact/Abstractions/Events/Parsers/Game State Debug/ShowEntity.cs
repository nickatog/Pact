using System.Collections.Generic;
using System.Text.RegularExpressions;

using Pact.Extensions.String;

namespace Pact.GameStateDebugEventParsers
{
    public sealed class ShowEntity
        : GameStateDebugEventParser
    {
        private static readonly Regex s_tagPattern =
            new Regex(@"^\s*tag=(?<tag>\S*) value=(?<value>.*)$", RegexOptions.Compiled);

        public ShowEntity()
            : base("SHOW_ENTITY - Updating (?<Attributes>.*)$") {}

        protected override IEnumerable<string> ParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            ParserContext parserContext,
            ref IEnumerable<object> parsedEvents)
        {
            IDictionary<string, string> attributes = parserContext.StartGroupValues["Attributes"].ParseKeyValuePairs();
            attributes.TryGetValue("CardID", out string cardID);

            attributes.TryGetValue("Entity", out string entity);
            IDictionary<string, string> entityAttributes = entity.Rem("[", "]").ParseKeyValuePairs();
            entityAttributes.TryGetValue("id", out string id);
            entityAttributes.TryGetValue("player", out string player);
            entityAttributes.TryGetValue("zone", out string zone);

            var events = new List<object>();
            var linesConsumed = new List<string>();
            var tags = new Dictionary<string, string>();

            while (!lines.HasCompleted)
            {
                string currentLine = lines.Current;

                // Check if the current line is outside the scope of this parser
                if (!parserContext.NestedOffsetPattern.IsMatch(currentLine))
                {
                    tags.TryGetValue("CONTROLLER", out string controller);
                    tags.TryGetValue("ZONE", out string zoneTag);

                    if (int.TryParse(entity, out int entityID))
                    {
                        parseContext.EntityMappings[entityID.ToString()] = cardID;

                        if (zoneTag.Eq("HAND"))
                        {
                            events.Add(new GameEvents.MulliganOptionPresented(cardID));

                            if (int.TryParse(controller, out int playerID) && parseContext.PlayerID == null)
                            {
                                parseContext.PlayerID = controller;

                                events.Add(new GameEvents.PlayerDetermined(playerID));
                            }
                        }
                    }
                    else if (int.TryParse(player, out int playerID))
                    {
                        parseContext.EntityMappings[id] = cardID;

                        if (zone.Eq("DECK"))
                        {
                            if (zoneTag.Eq("HAND"))
                                events.Add(new GameEvents.CardDrawnFromDeck(playerID, cardID));
                            else if (zoneTag.Eq("PLAY"))
                                events.Add(new GameEvents.CardEnteredPlayFromDeck(playerID, cardID));
                            else if (zoneTag.Eq("GRAVEYARD"))
                                events.Add(new GameEvents.CardOverdrawnFromDeck(playerID, cardID));
                            else if (zoneTag.Eq("SETASIDE"))
                                events.Add(new GameEvents.CardRemovedFromDeck(playerID, cardID));
                        }
                    }

                    if (parseContext.ParentBlock != null)
                    {
                        IDictionary<string, string> parentBlockAttributes = parseContext.ParentBlock.Attributes;
                        parentBlockAttributes.TryGetValue("BlockType", out string parentBlockType);
                        parentBlockAttributes.TryGetValue("Entity", out string parentBlockEntity);

                        IDictionary<string, string> parentBlockEntityAttributes = parentBlockEntity.ParseKeyValuePairs();
                        parentBlockEntityAttributes.TryGetValue("id", out string parentBlockEntityID);

                        if (parentBlockType.Eq("POWER") && parentBlockEntityID != null && parseContext.EntityMappings.TryGetValue(parentBlockEntityID, out string parentBlockEntityCardID) && int.TryParse(player, out int playerID))
                        {
                            // Skulking Geist
                            if (parentBlockEntityCardID.Eq("ICC_701"))
                                events.Add(new GameEvents.CardRemovedFromDeck(playerID, cardID));
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

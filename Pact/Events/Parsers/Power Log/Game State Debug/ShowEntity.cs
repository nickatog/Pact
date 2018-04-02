using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pact.StringExtensions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class ShowEntity
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_showEntityPattern =
            new Regex(@"^(?<Offset>\s*)SHOW_ENTITY - Updating (?<Attributes>.*)$", RegexOptions.Compiled);

        private static readonly Regex s_tagPattern =
            new Regex(@"^\s*tag=(?<tag>\S*) value=(?<value>.*)$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            Match match = s_showEntityPattern.Match(currentLine);
            if (!match.Success)
                return null;

            var nestedOffsetPattern = new Regex($@"^{match.Groups["Offset"].Value}    .*$");

            IDictionary<string, string> attributes = match.Groups["Attributes"].Value.ParseKeyValuePairs();
            attributes.TryGetValue("CardID", out string cardID);
            attributes.TryGetValue("Entity", out string entity);

            IDictionary<string, string> entityAttributes = entity.Replace("[", string.Empty).Replace("]", string.Empty).ParseKeyValuePairs();
            entityAttributes.TryGetValue("id", out string id);
            entityAttributes.TryGetValue("player", out string player);
            entityAttributes.TryGetValue("zone", out string zone);

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            var tags = new Dictionary<string, string>();
            Match tagMatch;

            while ((currentLine = lines.Current) != null)
            {
                if (!nestedOffsetPattern.IsMatch(currentLine))
                {
                    tags.TryGetValue("CONTROLLER", out string controller);
                    tags.TryGetValue("ZONE", out string zoneTag);

                    if (int.TryParse(entity, out int entityID))
                    {
                        parseContext.EntityMappings[entityID.ToString()] = cardID;
                        
                        if (zoneTag.Eq("HAND"))
                        {
                            events.Add(new Events.MulliganOptionPresented(cardID));

                            if (int.TryParse(controller, out int playerID) && parseContext.PlayerID == null)
                            {
                                parseContext.PlayerID = controller;

                                events.Add(new Events.PlayerDetermined(playerID));
                            }
                        }
                    }
                    else if (int.TryParse(player, out int playerID))
                    {
                        parseContext.EntityMappings[id] = cardID;

                        if (zone.Eq("DECK"))
                        {
                            if (zoneTag.Eq("HAND"))
                                events.Add(new Events.CardDrawnFromDeck(playerID, cardID));
                            else if (zoneTag.Eq("PLAY"))
                                events.Add(new Events.CardEnteredPlayFromDeck(playerID, cardID));

                            // if zoneTag.Eq("SETASIDE") then removed from deck? is there anything that would put it in setaside then back in the deck?
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
                                events.Add(new Events.CardRemovedFromDeck(playerID, cardID));
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

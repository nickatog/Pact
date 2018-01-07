using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pact.StringExtensions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class FullEntity
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_fullEntityPattern =
            new Regex(@"^(?<Offset>\s*)FULL_ENTITY - Creating .*$", RegexOptions.Compiled);

        private static readonly Regex s_tagPattern =
            new Regex(@"^\s*tag=(?<tag>\S*) value=(?<value>.*)$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            BlockContext parentBlock,
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            Match match = s_fullEntityPattern.Match(currentLine);
            if (!match.Success)
                return null;

            var nestedOffsetPattern = new Regex($@"^{match.Groups["Offset"].Value}    .*$");

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            IDictionary<string, string> fullEntityAttributes = currentLine.ParseKeyValuePairs();

            if (!fullEntityAttributes.TryGetValue("CardID", out string cardID))
                return linesConsumed;

            var tags = new Dictionary<string, string>();

            Match tagMatch;
            while ((currentLine = lines.Current) != null
                && nestedOffsetPattern.IsMatch(currentLine)
                && (tagMatch = s_tagPattern.Match(currentLine)).Success)
            {
                tags.Add(tagMatch.Groups["tag"].Value, tagMatch.Groups["value"].Value);

                linesConsumed.Add(currentLine);
                lines.MoveNext();
            }

            tags.TryGetValue("CONTROLLER", out string controllerText);
            int.TryParse(controllerText, out int playerID);

            if (tags.TryGetValue("CARDTYPE", out string cardType)
                && string.Equals(cardType, "HERO", StringComparison.Ordinal)
                && tags.TryGetValue("ENTITY_ID", out string entityIDText)
                && int.TryParse(entityIDText, out int entityID))
                events.Add(new Events.HeroEntityIdentified(playerID, entityID, cardID));

            if (parentBlock != null)
            {
                tags.TryGetValue("ZONE", out string zone);

                parentBlock.Attributes.TryGetValue("BlockType", out string parentBlockType);

                parentBlock.Attributes.TryGetValue("Entity", out string parentBlockEntity);
                IDictionary<string, string> parentEntityAttributes = parentBlockEntity.ParseKeyValuePairs();

                if (string.Equals(parentBlockType, "POWER", StringComparison.Ordinal)
                    && string.Equals(zone, "DECK", StringComparison.Ordinal))
                {
                    // create event based on the entity that created the new object, since the log doesn't tell us
                    parentEntityAttributes.TryGetValue("cardId", out string parentEntityCardID);

                    if (parentEntityCardID.Eq("CFM_602"))
                        events.Add(new Events.CardAddedToDeck(playerID, "CFM_602"));
                }
            }

            

            





            parsedEvents = events;

            return linesConsumed;
        }
    }
}

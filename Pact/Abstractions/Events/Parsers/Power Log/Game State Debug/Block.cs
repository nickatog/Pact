using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Pact.Extensions.String;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class Block
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_endPattern =
            new Regex(@"^\s*BLOCK_END.*$", RegexOptions.Compiled);

        private static readonly Regex s_startPattern =
            new Regex(@"^(?<Offset>\s*)BLOCK_START (?<Attributes>.*)$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            Match match = s_startPattern.Match(currentLine);
            if (!match.Success)
                return null;

            var nestedOffsetPattern = new Regex($@"^{match.Groups["Offset"].Value}    .*$");

            IDictionary<string, string> attributes = match.Groups["Attributes"].Value.ParseKeyValuePairs();
            attributes.TryGetValue("Entity", out string entity);
            attributes.TryGetValue("TriggerKeyword", out string triggerKeyword);

            IDictionary<string, string> entityAttributes = entity.Replace("[", string.Empty).Replace("]", string.Empty).ParseKeyValuePairs();
            entityAttributes.TryGetValue("id", out string entityID);
            entityAttributes.TryGetValue("player", out string entityPlayer);

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            BlockContext parentBlock = parseContext.ParentBlock;
            parseContext.ParentBlock = new BlockContext(attributes, parentBlock);

            while ((currentLine = lines.Current) != null)
            {
                if (s_endPattern.IsMatch(currentLine))
                {
                    linesConsumed.Add(currentLine);
                    lines.MoveNext();

                    parseContext.ParentBlock = parentBlock;

                    parsedEvents =
                        CreateBlockEvents()
                        .Concat(events)
                        .ToList();

                    return linesConsumed;
                }

                if (!nestedOffsetPattern.IsMatch(currentLine))
                {
                    parseContext.ParentBlock = parentBlock;

                    parsedEvents =
                        CreateBlockEvents()
                        .Concat(events)
                        .ToList();

                    return linesConsumed;
                }

                IEnumerable<string> innerLinesConsumed = null;

                foreach (IGameStateDebugEventParser parser in parseContext.Parsers)
                {
                    innerLinesConsumed = parser.TryParseEvents(lines, parseContext, out IEnumerable<object> innerEvents);
                    if (innerLinesConsumed == null)
                        continue;

                    linesConsumed.AddRange(innerLinesConsumed);

                    if (innerEvents == null)
                        return linesConsumed;

                    events.AddRange(innerEvents);

                    break;
                }

                if (innerLinesConsumed == null)
                {
                    linesConsumed.Add(currentLine);
                    lines.MoveNext();
                }
            }

            return linesConsumed;

            IEnumerable<object> CreateBlockEvents()
            {
                var blockEvents = new List<object>();

                if (triggerKeyword.Eq("TOPDECK"))
                {
                    if (int.TryParse(entityPlayer, out int playerID) && parseContext.EntityMappings.TryGetValue(entityID, out string cardID))
                        blockEvents.Add(new Events.CardRemovedFromDeck(playerID, cardID));
                }

                return blockEvents;
            }
        }
    }
}

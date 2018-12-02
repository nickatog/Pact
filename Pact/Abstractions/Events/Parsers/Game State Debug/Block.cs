using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Pact.Extensions.String;

namespace Pact.GameStateDebugEventParsers
{
    public sealed class Block
        : GameStateDebugEventParser
    {
        private static readonly Regex s_blockEndPattern =
            new Regex(@"^\s*BLOCK_END.*$", RegexOptions.Compiled);

        public Block()
            : base("BLOCK_START (?<Attributes>.*)$") {}

        protected override IEnumerable<string> ParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            ParserContext parserContext,
            ref IEnumerable<object> parsedEvents)
        {
            IDictionary<string, string> attributes = parserContext.StartGroupValues["Attributes"].ParseKeyValuePairs();
            attributes.TryGetValue("TriggerKeyword", out string triggerKeyword);

            attributes.TryGetValue("Entity", out string entity);
            IDictionary<string, string> entityAttributes = entity.Rem("[", "]").ParseKeyValuePairs();
            entityAttributes.TryGetValue("id", out string entityID);
            entityAttributes.TryGetValue("player", out string entityPlayer);

            int? playerID = null;
            if (int.TryParse(entityPlayer, out int parsedPlayerID))
                playerID = parsedPlayerID;

            parseContext.ParentBlock = new BlockContext(attributes, parseContext.ParentBlock);

            var events = new List<object>();
            var linesConsumed = new List<string>();

            while (!lines.HasCompleted)
            {
                string currentLine = lines.Current;

                // Check if the current line ends this parser
                if (s_blockEndPattern.IsMatch(currentLine))
                {
                    linesConsumed.Add(currentLine);
                    lines.MoveNext();

                    parseContext.ParentBlock = parseContext.ParentBlock.ParentBlock;

                    parsedEvents = __CreateBlockEvents().Concat(events);

                    return linesConsumed;
                }

                // Check if the current line is outside the scope of this parser
                if (!parserContext.NestedOffsetPattern.IsMatch(currentLine))
                {
                    parseContext.ParentBlock = parseContext.ParentBlock.ParentBlock;

                    parsedEvents = __CreateBlockEvents().Concat(events);

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

            IEnumerable<object> __CreateBlockEvents()
            {
                var blockEvents = new List<object>();

                if (triggerKeyword.Eq("TOPDECK"))
                {
                    if (playerID.HasValue && parseContext.EntityMappings.TryGetValue(entityID, out string cardID))
                        blockEvents.Add(new GameEvents.CardRemovedFromDeck(playerID.Value, cardID));
                }

                return blockEvents;
            }
        }
    }
}

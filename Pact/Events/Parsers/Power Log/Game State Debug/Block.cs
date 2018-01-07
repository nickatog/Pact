using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pact.StringExtensions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class Block
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_endPattern =
            new Regex(@"^\s*BLOCK_END.*$", RegexOptions.Compiled);

        private static readonly Regex s_startPattern =
            new Regex(@"^\s*BLOCK_START (?<Attributes>.*)$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            BlockContext parentBlock,
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            Match match = s_startPattern.Match(currentLine);
            if (!match.Success)
                return null;

            IDictionary<string, string> attributes = match.Groups["Attributes"].Value.ParseKeyValuePairs();

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            while ((currentLine = lines.Current) != null)
            {
                if (s_endPattern.IsMatch(currentLine))
                {
                    linesConsumed.Add(currentLine);
                    lines.MoveNext();

                    parsedEvents = events;

                    return linesConsumed;
                }

                IEnumerable<string> innerLinesConsumed = null;

                foreach (IGameStateDebugEventParser gameStateDebugEventParser in gameStateDebugEventParsers)
                {
                    innerLinesConsumed = gameStateDebugEventParser.TryParseEvents(lines, new BlockContext(attributes, parentBlock), gameStateDebugEventParsers, out IEnumerable<object> innerEvents);
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
        }
    }
}

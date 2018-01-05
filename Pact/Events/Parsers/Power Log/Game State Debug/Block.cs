using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class Block
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_endPattern = new Regex(@"^\s*BLOCK_END.*$", RegexOptions.Compiled);
        private static readonly Regex s_startPattern = new Regex(@"^\s*BLOCK_START.*$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            if (!s_startPattern.IsMatch(currentLine))
                return null;

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
                    innerLinesConsumed = gameStateDebugEventParser.TryParseEvents(lines, gameStateDebugEventParsers, out IEnumerable<object> innerEvents);
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

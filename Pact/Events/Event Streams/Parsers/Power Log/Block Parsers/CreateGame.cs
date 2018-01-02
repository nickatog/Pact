using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pact.Events.Parsers.PowerLog.BlockParsers
{
    public sealed class CreateGame
        : IPowerLogBlockParser
    {
        private static readonly Regex s_startPattern =
            new Regex(
                @"^.*GameState.DebugPrintPower\(\) - CREATE_GAME.*$",
                RegexOptions.Compiled);

        bool IPowerLogBlockParser.TryParseEvents(
            IEnumerator<string> lines,
            out IEnumerable<object> parsedEvents,
            out string unusedText)
        {
            parsedEvents = null;
            unusedText = string.Empty;

            if (!s_startPattern.IsMatch(lines.Current))
                return false;

            lines.MoveNext();

            parsedEvents = new List<object> { new object() };

            return true;
        }
    }
}

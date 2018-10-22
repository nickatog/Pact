using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class CreateGame
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_createGamePattern =
            new Regex(@"^(?<Offset>\s*)CREATE_GAME.*$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            Match match = s_createGamePattern.Match(currentLine);
            if (!match.Success)
                return null;

            var nestedOffsetPattern = new Regex($@"^{match.Groups["Offset"].Value}    .*$");

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object> { new Events.GameStarted() };

            parseContext.Reset();

            while ((currentLine = lines.Current) != null)
            {
                if (!nestedOffsetPattern.IsMatch(currentLine))
                {
                    parsedEvents = events;

                    return linesConsumed;
                }

                linesConsumed.Add(currentLine);
                lines.MoveNext();
            }

            return linesConsumed;
        }
    }
}

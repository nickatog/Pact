using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class PlayerID
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_playerIDPattern =
            new Regex(@"^\s*PlayerID=(?<PlayerID>\d+), PlayerName=(?<PlayerName>.+)$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            Match match = s_playerIDPattern.Match(currentLine);
            if (!match.Success)
                return null;

            string playerID = match.Groups["PlayerID"].Value;
            string playerName = match.Groups["PlayerName"].Value;

            var linesConsumed = new List<string> { lines.Current };
            lines.MoveNext();

            parseContext.PlayerNames[playerID] = playerName;

            parsedEvents = Enumerable.Empty<object>();

            return linesConsumed;
        }
    }
}

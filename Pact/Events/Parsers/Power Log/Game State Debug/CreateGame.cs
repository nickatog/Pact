using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pact.StringExtensions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class CreateGame
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_createGamePattern =
            new Regex(@"^(?<Offset>\s*)CREATE_GAME.*$", RegexOptions.Compiled);

        private static readonly Regex s_playerPattern =
            new Regex(@"^\s*Player (?<Attributes>.*)$", RegexOptions.Compiled);

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
            Match match = s_createGamePattern.Match(currentLine);
            if (!match.Success)
                return null;

            var nestedOffsetPattern = new Regex($@"^{match.Groups["Offset"].Value}    .*$");

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var players = new List<(int PlayerID, int HeroEntityID)>();

            while ((currentLine = lines.Current) != null)
            {
                if (!nestedOffsetPattern.IsMatch(currentLine))
                {
                    parsedEvents = new List<object> { new Events.GameStarted(players) };

                    return linesConsumed;
                }

                Match playerMatch = s_playerPattern.Match(currentLine);
                if (playerMatch.Success)
                {
                    var playerAttributes = currentLine.ParseKeyValuePairs();

                    linesConsumed.Add(currentLine);
                    lines.MoveNext();

                    if (!(playerAttributes.TryGetValue("PlayerID", out string playerIDText)
                          && int.TryParse(playerIDText, out int playerID)))
                        continue;

                    var tags = new Dictionary<string, string>();

                    Match tagMatch;
                    while ((currentLine = lines.Current) != null && (tagMatch = s_tagPattern.Match(currentLine)).Success)
                    {
                        tags.Add(tagMatch.Groups["tag"].Value, tagMatch.Groups["value"].Value);

                        linesConsumed.Add(currentLine);
                        lines.MoveNext();
                    }

                    if (!(tags.TryGetValue("HERO_ENTITY", out string heroEntityIDText)
                          && int.TryParse(heroEntityIDText, out int heroEntityID)))
                        continue;

                    players.Add((PlayerID: playerID, HeroEntityID: heroEntityID));

                    continue;
                }

                linesConsumed.Add(currentLine);
                lines.MoveNext();
            }

            return linesConsumed;
        }
    }
}

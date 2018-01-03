using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pact
{
    public sealed class GameStateDebugPowerLogEventParser
        : IPowerLogEventParser
    {
        private readonly IEnumerable<IGameStateDebugEventParser> _gameStateDebugEventParsers;

        public GameStateDebugPowerLogEventParser(
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers = null)
        {
            _gameStateDebugEventParsers = gameStateDebugEventParsers ?? Enumerable.Empty<IGameStateDebugEventParser>();
        }

        private static readonly Regex s_methodPattern =
            new Regex(
                @"^.*GameState.DebugPrintPower\(\) - .*$",
                RegexOptions.Compiled);

        IEnumerable<object> IPowerLogEventParser.ParseEvents(
            ref string text)
        {
            var allParsedEvents = new List<object>();

            IEnumerator<string> lines = GetLines(text);

            lines.MoveNext();
            while (lines.Current != null)
            {
                bool linesConsumed = false;

                foreach (IGameStateDebugEventParser gameStateDebugEventParser in _gameStateDebugEventParsers)
                {
                    linesConsumed = gameStateDebugEventParser.TryParseEvents(lines, out IEnumerable<object> parsedEvents, out text);
                    if (!linesConsumed)
                        continue;

                    if (parsedEvents == null)
                        return allParsedEvents;

                    allParsedEvents.AddRange(parsedEvents);

                    break;
                }

                if (!linesConsumed)
                    lines.MoveNext();
            }

            return allParsedEvents;

            IEnumerator<string> GetLines(string source)
            {
                using (var stringReader = new StringReader(source))
                {
                    string currentLine;
                    while ((currentLine = stringReader.ReadLine()) != null)
                        if (s_methodPattern.IsMatch(currentLine))
                            yield return currentLine;
                }

                yield return null;
            }
        }
    }
}

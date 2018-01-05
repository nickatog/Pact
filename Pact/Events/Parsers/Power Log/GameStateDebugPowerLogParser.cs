using System;
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
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers)
        {
            _gameStateDebugEventParsers =
                gameStateDebugEventParsers
                ?? Enumerable.Empty<IGameStateDebugEventParser>();
        }

        private const string LINE_PREFIX = "GameState.DebugPrintPower() - ";

        private static readonly Regex s_gameStateMethodPattern =
            new Regex(
                @"^.*GameState.DebugPrintPower\(\) - (?<Output>.*)$",
                RegexOptions.Compiled);

        IEnumerable<object> IPowerLogEventParser.ParseEvents(
            ref string text)
        {
            var allParsedEvents = new List<object>();

            IEnumerator<string> lines = GetLines(text);

            lines.MoveNext();
            while (lines.Current != null)
            {
                IEnumerable<string> linesConsumed = null;

                foreach (IGameStateDebugEventParser gameStateDebugEventParser in _gameStateDebugEventParsers)
                {
                    linesConsumed = gameStateDebugEventParser.TryParseEvents(lines, _gameStateDebugEventParsers, out IEnumerable<object> parsedEvents);
                    if (linesConsumed == null)
                        continue;

                    if (parsedEvents == null)
                    {
                        text = string.Join(Environment.NewLine, linesConsumed.Select(__line => LINE_PREFIX + __line));

                        return allParsedEvents;
                    }

                    allParsedEvents.AddRange(parsedEvents);

                    break;
                }

                if (linesConsumed == null)
                    lines.MoveNext();
            }

            text = null;

            return allParsedEvents;

            IEnumerator<string> GetLines(string source)
            {
                using (var stringReader = new StringReader(source))
                {
                    string currentLine;
                    Match match;
                    while ((currentLine = stringReader.ReadLine()) != null)
                        if ((match = s_gameStateMethodPattern.Match(currentLine)).Success)
                            yield return match.Groups["Output"].Value;
                }

                yield return null;
            }
        }
    }
}

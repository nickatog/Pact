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
        private static readonly Regex s_gameStateMethodPattern =
            new Regex(@"^.*GameState.DebugPrint(Power|Game)\(\) - (?<Output>.*)$", RegexOptions.Compiled);

        private const string LINE_PREFIX = "GameState.DebugPrintPower() - ";

        private readonly IList<IGameStateDebugEventParser> _gameStateDebugEventParsers;

        private readonly ParseContext _parseContext;

        public GameStateDebugPowerLogEventParser(
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers)
        {
            _gameStateDebugEventParsers = (gameStateDebugEventParsers ?? Enumerable.Empty<IGameStateDebugEventParser>()).ToList();

            _parseContext = new ParseContext(_gameStateDebugEventParsers);
        }

        IEnumerable<object> IPowerLogEventParser.ParseEvents(
            ref string text)
        {
            var allParsedEvents = new List<object>();

            using (var lines = new TrackingEnumerator<string>(__GetLines(text)))
            {
                lines.MoveNext();
                while (!lines.HasCompleted)
                {
                    IEnumerable<string> linesConsumed = null;

                    foreach (IGameStateDebugEventParser gameStateDebugEventParser in _gameStateDebugEventParsers)
                    {
                        linesConsumed = gameStateDebugEventParser.TryParseEvents(lines, _parseContext, out IEnumerable<object> parsedEvents);
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
            }

            text = null;

            return allParsedEvents;

            IEnumerator<string> __GetLines(
                string source)
            {
                using (var stringReader = new StringReader(source))
                {
                    string currentLine;
                    Match match;
                    while ((currentLine = stringReader.ReadLine()) != null)
                        if ((match = s_gameStateMethodPattern.Match(currentLine)).Success)
                            yield return match.Groups["Output"].Value;
                }
            }
        }
    }
}

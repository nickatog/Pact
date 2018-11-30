using System;
using System.Collections;
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
            new Regex(
                @"^.*GameState.DebugPrint(Power|Game)\(\) - (?<Output>.*)$",
                RegexOptions.Compiled);

        private const string LINE_PREFIX = "GameState.DebugPrintPower() - ";

        private readonly IEnumerable<IGameStateDebugEventParser> _gameStateDebugEventParsers;
        private readonly ParseContext _parseContext;

        public GameStateDebugPowerLogEventParser(
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers)
        {
            _gameStateDebugEventParsers = gameStateDebugEventParsers ?? Enumerable.Empty<IGameStateDebugEventParser>();

            _parseContext = new ParseContext(_gameStateDebugEventParsers);
        }

        IEnumerable<object> IPowerLogEventParser.ParseEvents(
            ref string text)
        {
            var allParsedEvents = new List<object>();

            using (var lines = new TrackingEnumerator<string>(__GetLines(text)))
            {
                lines.MoveNext();
                while (lines.Current != null)
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

                yield return null;
            }
        }

        private sealed class TrackingEnumerator<T>
            : IEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public TrackingEnumerator(
                IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            public bool HasCompleted { get; private set; }

            public T Current => _enumerator.Current;

            object IEnumerator.Current => _enumerator.Current;

            void IDisposable.Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return HasCompleted = !_enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}

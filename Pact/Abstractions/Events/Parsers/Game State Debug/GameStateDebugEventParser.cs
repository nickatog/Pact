using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pact
{
    public abstract class GameStateDebugEventParser
        : IGameStateDebugEventParser
    {
        private const string EXPRESSION_OFFSET = @"^(?<Offset>\s*)";

        private readonly Regex _startPattern;

        protected GameStateDebugEventParser(
            string startExpressionSuffix)
        {
            _startPattern = new Regex(EXPRESSION_OFFSET + startExpressionSuffix, RegexOptions.Compiled);
        }

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            // Check if the current line starts this parser
            string currentLine = lines.Current;
            Match match = _startPattern.Match(currentLine);
            if (!match.Success)
                return null;

            // Start tracking lines consumed from the enumerator
            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            // Create a text pattern to detect when to stop parsing
            var nestedOffsetPattern = new Regex($@"^{match.Groups["Offset"].Value}    .*$");

            // Store each group value for later use
            var startGroupValues = new Dictionary<string, string>();
            foreach (Group group in match.Groups)
                startGroupValues.Add(group.Name, group.Value);

            IEnumerable<string> innerLinesConsumed =
                ParseEvents(
                    lines,
                    parseContext,
                    new ParserContext(
                        currentLine,
                        startGroupValues,
                        nestedOffsetPattern),
                    ref parsedEvents);

            return linesConsumed.Concat(innerLinesConsumed ?? Enumerable.Empty<string>());
        }

        protected abstract IEnumerable<string> ParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            ParserContext parserContext,
            ref IEnumerable<object> parsedEvents);
    }
}

using System.Collections.Generic;

namespace Pact.GameStateDebugEventParsers
{
    public sealed class CreateGame
        : GameStateDebugEventParser
    {
        public CreateGame()
            : base("CREATE_GAME.*$") { }

        protected override IEnumerable<string> ParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            ParserContext parserContext,
            ref IEnumerable<object> parsedEvents)
        {
            parseContext.Reset();

            var events = new List<object> { new GameEvents.GameStarted() };
            var linesConsumed = new List<string>();

            while (!lines.HasCompleted)
            {
                string currentLine = lines.Current;

                // Check if the current line is outside the scope of this parser
                if (!parserContext.NestedOffsetPattern.IsMatch(currentLine))
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

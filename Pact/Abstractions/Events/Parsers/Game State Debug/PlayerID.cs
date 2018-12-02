using System.Collections.Generic;
using System.Linq;

namespace Pact.GameStateDebugEventParsers
{
    public sealed class PlayerID
        : GameStateDebugEventParser
    {
        public PlayerID()
            : base(@"PlayerID=(?<PlayerID>\d+), PlayerName=(?<PlayerName>.+)$") {}

        protected override IEnumerable<string> ParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            ParserContext parserContext,
            ref IEnumerable<object> parsedEvents)
        {
            string playerID = parserContext.StartGroupValues["PlayerID"];
            string playerName = parserContext.StartGroupValues["PlayerName"];

            parseContext.PlayerNames[playerID] = playerName;

            parsedEvents = Enumerable.Empty<object>();

            return Enumerable.Empty<string>();
        }
    }
}

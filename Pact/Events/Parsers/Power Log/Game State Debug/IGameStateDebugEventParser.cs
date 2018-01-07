using System.Collections.Generic;

namespace Pact
{
    public interface IGameStateDebugEventParser
    {
        IEnumerable<string> TryParseEvents(
            IEnumerator<string> lines,
            BlockContext parentBlock,
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers,
            out IEnumerable<object> parsedEvents);
    }
}

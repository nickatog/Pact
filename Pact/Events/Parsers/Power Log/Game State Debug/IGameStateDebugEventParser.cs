using System.Collections.Generic;

namespace Pact
{
    public interface IGameStateDebugEventParser
    {
        IEnumerable<string> TryParseEvents(
            IEnumerator<string> lines,
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers,
            out IEnumerable<object> parsedEvents);
    }
}

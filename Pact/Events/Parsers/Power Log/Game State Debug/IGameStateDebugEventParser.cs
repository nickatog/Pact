using System.Collections.Generic;

namespace Pact
{
    public interface IGameStateDebugEventParser
    {
        bool TryParseEvents(
            IEnumerator<string> lines,
            out IEnumerable<object> parsedEvents,
            out string unusedText);
    }
}

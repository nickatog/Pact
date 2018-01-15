using System.Collections.Generic;

namespace Pact
{
    public interface IGameStateDebugEventParser
    {
        IEnumerable<string> TryParseEvents(
            IEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents);
    }
}

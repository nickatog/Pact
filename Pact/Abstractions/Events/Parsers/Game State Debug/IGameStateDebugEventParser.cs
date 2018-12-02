using System.Collections.Generic;

namespace Pact
{
    public interface IGameStateDebugEventParser
    {
        IEnumerable<string> TryParseEvents(
            TrackingEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents);
    }
}

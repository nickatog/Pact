using System.Collections.Generic;

namespace Pact
{
    public interface IPowerLogBlockParser
    {
        bool TryParseEvents(
            IEnumerator<string> lines,
            out IEnumerable<object> parsedEvents,
            out string unusedText);
    }
}

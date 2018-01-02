using System.Collections.Generic;

namespace Pact
{
    public interface IPowerLogParser
    {
        IEnumerable<object> ParseEvents(
            ref string text);
    }
}

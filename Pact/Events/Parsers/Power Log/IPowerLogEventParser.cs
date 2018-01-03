using System.Collections.Generic;

namespace Pact
{
    public interface IPowerLogEventParser
    {
        IEnumerable<object> ParseEvents(
            ref string text);
    }
}

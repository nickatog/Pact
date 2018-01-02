using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pact
{
    public sealed class PowerLogParser
        : IPowerLogParser
    {
        private readonly IEnumerable<IPowerLogBlockParser> _powerLogBlockParsers;

        public PowerLogParser(
            IEnumerable<IPowerLogBlockParser> powerLogBlockParsers = null)
        {
            _powerLogBlockParsers = powerLogBlockParsers ?? Enumerable.Empty<IPowerLogBlockParser>();
        }

        IEnumerable<object> IPowerLogParser.ParseEvents(
            ref string text)
        {
            var allParsedEvents = new List<object>();

            IEnumerator<string> lines = GetLines(text);

            lines.MoveNext();
            while (lines.Current != null)
            {
                bool linesRead = false;

                foreach (IPowerLogBlockParser powerLogBlockParser in _powerLogBlockParsers)
                {
                    linesRead = powerLogBlockParser.TryParseEvents(lines, out IEnumerable<object> parsedEvents, out text);
                    if (!linesRead)
                        continue;

                    if (parsedEvents == null)
                        return allParsedEvents;

                    allParsedEvents.AddRange(parsedEvents);

                    break;
                }

                if (!linesRead)
                    lines.MoveNext();
            }

            return allParsedEvents;

            IEnumerator<string> GetLines(string source)
            {
                using (var stringReader = new StringReader(source))
                {
                    string currentLine;
                    while ((currentLine = stringReader.ReadLine()) != null)
                        yield return currentLine;
                }

                yield return null;
            }
        }
    }
}

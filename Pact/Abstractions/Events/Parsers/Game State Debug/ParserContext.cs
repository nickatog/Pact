using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pact
{
    public struct ParserContext
    {
        public ParserContext(
            string startLine,
            IDictionary<string, string> startGroupValues,
            Regex nestedOffsetPattern)
        {
            StartLine = startLine;
            StartGroupValues = startGroupValues;
            NestedOffsetPattern = nestedOffsetPattern;
        }

        public Regex NestedOffsetPattern { get; }
        public IDictionary<string, string> StartGroupValues { get; }
        public string StartLine { get; }
    }
}

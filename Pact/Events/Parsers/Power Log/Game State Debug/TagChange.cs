using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pact.StringExtensions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class TagChange
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_tagChangePattern =
            new Regex(@"^\s*TAG_CHANGE .*$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            BlockContext parentBlock,
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            if (!s_tagChangePattern.IsMatch(currentLine))
                return null;

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            IDictionary<string, string> tagChangeAttributes = currentLine.ParseKeyValuePairs();

            if (tagChangeAttributes.TryGetValue("tag", out string tag)
                && string.Equals(tag, "PLAYSTATE", StringComparison.Ordinal))
            {
                tagChangeAttributes.TryGetValue("Entity", out string entityName);

                tagChangeAttributes.TryGetValue("value", out string value);
                if (string.Equals(value, "WON", StringComparison.Ordinal))
                    events.Add(new Events.GameWon(entityName));
                else if (string.Equals(value, "LOST", StringComparison.Ordinal))
                    events.Add(new Events.GameLost(entityName));
            }

            parsedEvents = events;

            return linesConsumed;
        }
    }
}

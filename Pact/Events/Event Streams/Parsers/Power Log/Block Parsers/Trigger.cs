using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pact.StringExtensions;

namespace Pact.Events.Parsers.PowerLog.BlockParsers
{
    public sealed class Trigger
        : IPowerLogBlockParser
    {
        private static readonly Regex s_endPattern =
            new Regex(
                @"^.*GameState.DebugPrintPower\(\) - BLOCK_END.*$",
                RegexOptions.Compiled);

        private static readonly Regex s_showEntityUpdatingPattern =
            new Regex(
                @"^.*GameState.DebugPrintPower\(\) -     SHOW_ENTITY - Updating.*$",
                RegexOptions.Compiled);

        private static readonly Regex s_startPattern =
            new Regex(
                @"^.*GameState.DebugPrintPower\(\) - BLOCK_START BlockType=TRIGGER.*$",
                RegexOptions.Compiled);

        private static readonly Regex s_tagChangePattern =
            new Regex(
                @"^.*GameState.DebugPrintPower\(\) -     TAG_CHANGE.*$",
                RegexOptions.Compiled);

        bool IPowerLogBlockParser.TryParseEvents(
            IEnumerator<string> lines,
            out IEnumerable<object> parsedEvents,
            out string unusedText)
        {
            parsedEvents = null;
            unusedText = string.Empty;

            string currentLine = lines.Current;
            Match match = s_startPattern.Match(currentLine);
            if (!match.Success)
                return false;

            IDictionary<string, string> blockAttributes = currentLine.ParseKeyValuePairs();

            unusedText += currentLine;

            var tempEvents = new List<object>();

            while (lines.MoveNext() && (currentLine = lines.Current) != null)
            {
                if (s_endPattern.IsMatch(currentLine))
                {
                    lines.MoveNext();

                    parsedEvents = tempEvents;
                    unusedText = string.Empty;

                    return true;
                }
                else if (s_tagChangePattern.IsMatch(currentLine))
                {
                    IDictionary<string, string> tagChangeAttributes = currentLine.ParseKeyValuePairs();

                    // ???
                }
                else if (s_showEntityUpdatingPattern.IsMatch(currentLine))
                {
                    // player card draw?
                    IDictionary<string, string> showEntityUpdatingAttributes = currentLine.ParseKeyValuePairs();

                    if (showEntityUpdatingAttributes.TryGetValue("Entity", out string entity))
                    {
                        if (int.TryParse(entity, out int entityID))
                        {
                            // identifying existing entity (already in hand due to mulligan, other object, etc.)
                        }
                        else
                        {
                            IDictionary<string, string> entityAttributes =
                                entity
                                .Replace("[", string.Empty)
                                .Replace("]", string.Empty)
                                .ParseKeyValuePairs();

                            System.Diagnostics.Debug.WriteLine($"{showEntityUpdatingAttributes["CardID"]}");
                        }
                    }
                }

                unusedText += currentLine;
            }

            return true;
        }
    }
}

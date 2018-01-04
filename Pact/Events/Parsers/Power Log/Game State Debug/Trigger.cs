//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using Pact.StringExtensions;

//namespace Pact.EventParsers.PowerLog.GameStateDebug
//{
//    public sealed class Trigger
//        : IGameStateDebugEventParser
//    {
//        private static readonly Regex s_endPattern =
//            new Regex(
//                @"^.*GameState.DebugPrintPower\(\) - BLOCK_END.*$",
//                RegexOptions.Compiled);

//        private static readonly Regex s_showEntityUpdatingPattern =
//            new Regex(
//                @"^.*GameState.DebugPrintPower\(\) -     SHOW_ENTITY - Updating.*$",
//                RegexOptions.Compiled);

//        private static readonly Regex s_startPattern =
//            new Regex(
//                @"^.*GameState.DebugPrintPower\(\) - BLOCK_START BlockType=TRIGGER.*$",
//                RegexOptions.Compiled);

//        private static readonly Regex s_tagPattern =
//            new Regex(
//                @"^.*GameState.DebugPrintPower\(\) -         tag=(?<tag>\S*) value=(?<value>.*)$",
//                RegexOptions.Compiled);

//        private static readonly Regex s_tagChangePattern =
//            new Regex(
//                @"^.*GameState.DebugPrintPower\(\) -     TAG_CHANGE.*$",
//                RegexOptions.Compiled);

//        bool IGameStateDebugEventParser.TryParseEvents(
//            IEnumerator<string> lines,
//            out IEnumerable<object> parsedEvents,
//            out string unusedText)
//        {
//            parsedEvents = null;
//            unusedText = string.Empty;

//            string currentLine = lines.Current;
//            Match match = s_startPattern.Match(currentLine);
//            if (!match.Success)
//                return false;

//            var tempEvents = new List<object>();

//            IDictionary<string, string> blockAttributes = currentLine.ParseKeyValuePairs();

//            unusedText = currentLine;

//            lines.MoveNext();

//            currentLine = lines.Current;
//            if (currentLine == null)
//                return true;

//            do
//            {
//                if (s_endPattern.IsMatch(currentLine))
//                {
//                    lines.MoveNext();

//                    parsedEvents = tempEvents;
//                    unusedText = null;

//                    return true;
//                }
//                else if (s_tagChangePattern.IsMatch(currentLine))
//                {
//                    IDictionary<string, string> tagChangeAttributes = currentLine.ParseKeyValuePairs();

//                    // ???
//                }
//                else if (s_showEntityUpdatingPattern.IsMatch(currentLine))
//                {
//                    // player card draw?
//                    IDictionary<string, string> showEntityUpdatingAttributes = currentLine.ParseKeyValuePairs();

//                    showEntityUpdatingAttributes.TryGetValue("CardID", out string cardID);

//                    if (showEntityUpdatingAttributes.TryGetValue("Entity", out string entity))
//                    {
//                        if (int.TryParse(entity, out int entityID))
//                        {
//                            // identifying existing entity (already in hand due to mulligan, other object, etc.)
//                        }
//                        else
//                        {
//                            IDictionary<string, string> entityAttributes =
//                                entity
//                                .Replace("[", string.Empty)
//                                .Replace("]", string.Empty)
//                                .ParseKeyValuePairs();

//                            entityAttributes.TryGetValue("player", out string playerID);
//                            entityAttributes.TryGetValue("zone", out string oldZone);

//                            var tags = new Dictionary<string, string>();

//                            unusedText += System.Environment.NewLine + currentLine;

//                            Match tagMatch;
//                            while (lines.MoveNext() && (currentLine = lines.Current) != null && (tagMatch = s_tagPattern.Match(currentLine)).Success)
//                            {
//                                tags.Add(tagMatch.Groups["tag"].Value, tagMatch.Groups["value"].Value);

//                                unusedText += System.Environment.NewLine + currentLine;
//                            }

//                            tags.TryGetValue("ZONE", out string newZone);

//                            if (string.Equals(oldZone, "DECK", System.StringComparison.Ordinal) && string.Equals(newZone, "HAND", System.StringComparison.Ordinal))
//                                tempEvents.Add(
//                                    new Events.CardDrawnFromDeck(
//                                        playerID,
//                                        cardID));
//                        }
//                    }
//                }

//                unusedText += System.Environment.NewLine + currentLine;
//            } while (lines.MoveNext() && (currentLine = lines.Current) != null);

//            return true;
//        }
//    }
//}

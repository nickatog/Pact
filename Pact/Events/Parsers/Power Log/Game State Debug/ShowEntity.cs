using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pact.StringExtensions;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class ShowEntity
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_showEntityUpdatingPattern =
            new Regex(@"^\s*SHOW_ENTITY - Updating (?<Attributes>.*)$", RegexOptions.Compiled);

        private static readonly Regex s_tagPattern =
            new Regex(@"^\s*tag=(?<tag>\S*) value=(?<value>.*)$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            BlockContext parentBlock,
            IEnumerable<IGameStateDebugEventParser> gameStateDebugEventParsers,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            Match match = s_showEntityUpdatingPattern.Match(currentLine);
            if (!match.Success)
                return null;

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            IDictionary<string, string> showEntityUpdatingAttributes = currentLine.ParseKeyValuePairs();

            showEntityUpdatingAttributes.TryGetValue("CardID", out string cardID);

            if (showEntityUpdatingAttributes.TryGetValue("Entity", out string entity))
            {
                IDictionary<string, string> entityAttributes =
                    entity
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .ParseKeyValuePairs();

                var tags = new Dictionary<string, string>();

                Match tagMatch;
                while ((currentLine = lines.Current) != null && (tagMatch = s_tagPattern.Match(currentLine)).Success)
                {
                    tags.Add(tagMatch.Groups["tag"].Value, tagMatch.Groups["value"].Value);

                    linesConsumed.Add(currentLine);
                    lines.MoveNext();
                }

                if (!tags.TryGetValue("ZONE", out string newZone))
                    return linesConsumed;

                if (int.TryParse(entity, out int entityID))
                {
                    if (string.Equals(newZone, "HAND", StringComparison.Ordinal))
                    {
                        events.Add(new Events.MulliganOptionPresented(cardID));

                        if (tags.TryGetValue("CONTROLLER", out string controllerText)
                            && int.TryParse(controllerText, out int playerID))
                            events.Add(new Events.PlayerDetermined(playerID));
                    }
                }
                else
                {
                    if (!(entityAttributes.TryGetValue("player", out string playerIDText)
                          && int.TryParse(playerIDText, out int playerID)))
                        return linesConsumed;

                    entityAttributes.TryGetValue("zone", out string oldZone);

                    if (string.Equals(oldZone, "DECK", StringComparison.Ordinal))
                    {
                        if (string.Equals(newZone, "HAND", StringComparison.Ordinal))
                            events.Add(new Events.CardDrawnFromDeck(playerID, cardID));
                        else if (string.Equals(newZone, "PLAY", StringComparison.Ordinal))
                            events.Add(new Events.CardEnteredPlayFromDeck(playerID, cardID));
                    }
                }
            }

            parsedEvents = events;

            return linesConsumed;
        }
    }
}

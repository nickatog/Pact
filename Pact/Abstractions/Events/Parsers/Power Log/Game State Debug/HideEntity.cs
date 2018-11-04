using System.Collections.Generic;
using System.Text.RegularExpressions;

using Pact.Extensions.String;

namespace Pact.EventParsers.PowerLog.GameStateDebug
{
    public sealed class HideEntity
        : IGameStateDebugEventParser
    {
        private static readonly Regex s_hideEntityPattern =
            new Regex(@"^\s*HIDE_ENTITY - .*$", RegexOptions.Compiled);

        IEnumerable<string> IGameStateDebugEventParser.TryParseEvents(
            IEnumerator<string> lines,
            ParseContext parseContext,
            out IEnumerable<object> parsedEvents)
        {
            parsedEvents = null;

            string currentLine = lines.Current;
            if (!s_hideEntityPattern.IsMatch(currentLine))
                return null;

            var linesConsumed = new List<string> { currentLine };
            lines.MoveNext();

            var events = new List<object>();

            IDictionary<string, string> hideEntityAttributes = currentLine.ParseKeyValuePairs();

            hideEntityAttributes.TryGetValue("Entity", out string entity);
            IDictionary<string, string> entityAttributes = entity.Replace("[", string.Empty).Replace("]", string.Empty).ParseKeyValuePairs();
            entityAttributes.TryGetValue("zone", out string zone);
            entityAttributes.TryGetValue("cardId", out string cardID);
            entityAttributes.TryGetValue("player", out string player);

            hideEntityAttributes.TryGetValue("tag", out string tag);
            hideEntityAttributes.TryGetValue("value", out string value);

            if (tag.Eq("ZONE") && value.Eq("DECK") && zone.Eq("HAND"))
            {
                int.TryParse(player, out int playerID);

                events.Add(new Events.CardReturnedToDeckFromHand(playerID, cardID));
            }

            parsedEvents = events;

            return linesConsumed;
        }
    }
}

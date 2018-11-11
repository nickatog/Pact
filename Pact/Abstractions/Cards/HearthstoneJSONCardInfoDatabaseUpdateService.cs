using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class HearthstoneJSONCardInfoDatabaseUpdateService
        : ICardInfoDatabaseUpdateService
    {
        private static readonly Regex s_versionPattern = new Regex(@"(?<Version>\d+).*", RegexOptions.Compiled);

        private const string LATEST_PATH = "https://api.hearthstonejson.com/v1/latest/";

        async Task<int?> ICardInfoDatabaseUpdateService.GetLatestVersion()
        {
#if DEBUG
            ServicePointManager.SecurityProtocol |=
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif

            await Task.Delay(2000);

            string versionSegment = null;

            using (var httpClient = new HttpClient())
            using (HttpResponseMessage response = (await httpClient.GetAsync(LATEST_PATH)).EnsureSuccessStatusCode())
                versionSegment = response.RequestMessage.RequestUri.Segments.LastOrDefault();

            if (versionSegment == null)
                return null;

            Match patternMatch = s_versionPattern.Match(versionSegment);
            if (!patternMatch.Success)
                return null;

            if (!int.TryParse(patternMatch.Groups["Version"].Value, out int version))
                return null;

            return version;
        }

        async Task<Stream> ICardInfoDatabaseUpdateService.GetLatestVersionStream()
        {
#if DEBUG
            ServicePointManager.SecurityProtocol |=
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif

            using (var httpClient = new HttpClient())
                using (HttpResponseMessage response = (await httpClient.GetAsync($"{LATEST_PATH}enUS/cards.json")).EnsureSuccessStatusCode())
                    return await response.Content.ReadAsStreamAsync();
        }
    }
}

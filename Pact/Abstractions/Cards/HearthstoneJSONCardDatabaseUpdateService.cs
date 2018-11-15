using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class HearthstoneJSONCardDatabaseUpdateService
        : ICardDatabaseUpdateService
    {
        private static readonly HttpClient s_httpClient = new HttpClient();
        private static readonly Regex s_versionPattern = new Regex(@"(?<Version>\d+).*", RegexOptions.Compiled);

        private const string BASE_PATH = "https://api.hearthstonejson.com/v1/";

        async Task<int?> ICardDatabaseUpdateService.GetLatestVersion()
        {
#if DEBUG
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif

            string versionSegment = null;

            Task<HttpResponseMessage> requestTask = s_httpClient.GetAsync(BASE_PATH + "latest/");
            using (HttpResponseMessage response = (await requestTask.ConfigureAwait(false)).EnsureSuccessStatusCode())
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

        async Task<Stream> ICardDatabaseUpdateService.GetVersionStream(
            int version)
        {
#if DEBUG
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif

            Task<HttpResponseMessage> requestTask = s_httpClient.GetAsync($"{BASE_PATH}{version}/enUS/cards.json");
            HttpResponseMessage response = (await requestTask.ConfigureAwait(false)).EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }
}

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
        private static readonly HttpClient s_httpClient = new HttpClient();
        private static readonly Regex s_versionPattern = new Regex(@"(?<Version>\d+).*", RegexOptions.Compiled);

        private const string BASE_PATH = "https://api.hearthstonejson.com/v1/";

        async Task<int?> ICardInfoDatabaseUpdateService.GetLatestVersion()
        {
#if DEBUG
            ServicePointManager.SecurityProtocol |=
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif

            await Task.Delay(2000);

            string versionSegment = null;

            using (HttpResponseMessage response = (await s_httpClient.GetAsync(BASE_PATH + "latest/")).EnsureSuccessStatusCode())
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

        async Task<Stream> ICardInfoDatabaseUpdateService.GetVersionStream(
            int version)
        {
#if DEBUG
            ServicePointManager.SecurityProtocol |=
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif

            await Task.Delay(2000);

            HttpResponseMessage response = (await s_httpClient.GetAsync($"{BASE_PATH}{version}/enUS/cards.json")).EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class HearthstoneJSONCardDatabaseUpdateService
        : ICardDatabaseUpdateService
    {
        private static readonly HttpClient s_httpClient = new HttpClient();
        private static readonly Regex s_versionPattern = new Regex(@"(?<Version>\d+).*", RegexOptions.Compiled);

        private readonly string _baseURI;

        public HearthstoneJSONCardDatabaseUpdateService(
            string baseURI)
        {
            _baseURI = baseURI.Require(nameof(baseURI));
        }

        async Task<int?> ICardDatabaseUpdateService.GetLatestVersion()
        {
            string versionSegment = null;

            Task<HttpResponseMessage> requestTask = s_httpClient.GetAsync($"{_baseURI}latest/");
            using (HttpResponseMessage response = (await requestTask.ConfigureAwait(false)).EnsureSuccessStatusCode())
                versionSegment = response.RequestMessage.RequestUri.Segments.LastOrDefault();

            if (versionSegment == null)
                return null;

            Match patternMatch = s_versionPattern.Match(versionSegment);
            if (!patternMatch.Success)
                return null;

            if (int.TryParse(patternMatch.Groups["Version"].Value, out int version))
                return version;

            return null;
        }

        async Task<Stream> ICardDatabaseUpdateService.GetVersionStream(
            int version)
        {
            Task<HttpResponseMessage> requestTask = s_httpClient.GetAsync($"{_baseURI}{version}/enUS/cards.json");
            HttpResponseMessage response = (await requestTask.ConfigureAwait(false)).EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }
}

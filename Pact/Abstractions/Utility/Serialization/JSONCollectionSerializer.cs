using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class JSONCollectionSerializer<T>
        : ICollectionSerializer<T>
    {
        private readonly Encoding _encoding;

        public JSONCollectionSerializer(
            Encoding encoding = null)
        {
            _encoding = encoding ?? Encoding.Default;
        }

        Task<IEnumerable<T>> ICollectionSerializer<T>.Deserialize(
            Stream stream)
        {
            stream.Require(nameof(stream));

            return __Deserialize();

            async Task<IEnumerable<T>> __Deserialize()
            {
                using (var reader = new StreamReader(stream, _encoding))
                    return JsonConvert.DeserializeObject<IEnumerable<T>>(await reader.ReadToEndAsync().ConfigureAwait(false));
            }
        }

        Task ICollectionSerializer<T>.Serialize(
            Stream stream,
            IEnumerable<T> items)
        {
            stream.Require(nameof(stream));
            items.Require(nameof(items));

            return __Serialize();

            async Task __Serialize()
            {
                using (var writer = new StreamWriter(stream, _encoding))
                    await writer.WriteAsync(JsonConvert.SerializeObject(items)).ConfigureAwait(false);
            }
        }
    }
}

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class JSONSerializer<T>
        : ISerializer<T>
    {
        private readonly Encoding _encoding;

        public JSONSerializer(
            Encoding encoding = null)
        {
            _encoding = encoding ?? Encoding.Default;
        }

        Task<T> ISerializer<T>.Deserialize(
            Stream stream)
        {
            stream.Require(nameof(stream));

            return __Deserialize();

            async Task<T> __Deserialize()
            {
                using (var reader = new StreamReader(stream, _encoding))
                    return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync().ConfigureAwait(false));
            }
        }

        Task ISerializer<T>.Serialize(
            Stream stream,
            T item)
        {
            stream.Require(nameof(stream));

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return __Serialize();

            async Task __Serialize()
            {
                using (var writer = new StreamWriter(stream, _encoding))
                    await writer.WriteAsync(JsonConvert.SerializeObject(item)).ConfigureAwait(false);
            }
        }
    }
}

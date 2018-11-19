using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class TextDecklistSerializer
        : IDecklistSerializer
    {
        private readonly Encoding _encoding;
        private readonly IDecklistSerializer _serializer;

        public TextDecklistSerializer(
            IDecklistSerializer serializer,
            Encoding encoding = null)
        {
            _serializer = serializer.Require(nameof(serializer));
            _encoding = encoding ?? Encoding.Default;
        }

        Task<Decklist> IDecklistSerializer.Deserialize(
            Stream stream)
        {
            stream.Require(nameof(stream));

            return __Deserialize();

            async Task<Decklist> __Deserialize()
            {
                string deckstring = null;
                using (var reader = new StreamReader(stream, _encoding))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (line.Length <= 0 || line[0] == '#')
                            continue;

                        deckstring = line;

                        break;
                    }
                }

                if (deckstring == null)
                    throw new Exception("Text did not contain a deckstring!");

                using (var dataStream = new MemoryStream(Convert.FromBase64String(deckstring)))
                    return await _serializer.Deserialize(dataStream).ConfigureAwait(false);
            }
        }

        Task IDecklistSerializer.Serialize(
            Stream stream,
            Decklist decklist)
        {
            stream.Require(nameof(stream));

            return __Serialize();

            async Task __Serialize()
            {
                byte[] bytes;
                using (var dataStream = new MemoryStream())
                {
                    await _serializer.Serialize(dataStream, decklist).ConfigureAwait(false);

                    dataStream.Position = 0;

                    bytes = new byte[dataStream.Length];
                    dataStream.Read(bytes, 0, (int)dataStream.Length);
                }

                byte[] encodedBytes = _encoding.GetBytes(Convert.ToBase64String(bytes));

                await stream.WriteAsync(encodedBytes, 0, encodedBytes.Length).ConfigureAwait(false);
            }
        }
    }
}

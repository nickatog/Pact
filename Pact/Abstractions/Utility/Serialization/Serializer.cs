using System;
using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class Serializer<T>
        : ISerializer<T>
        where T : ISerializable<T>
    {
        private readonly Func<Stream, Task<T>> _deserialize;

        public Serializer(
            Func<Stream, Task<T>> deserialize)
        {
            _deserialize = deserialize ?? throw new ArgumentNullException(nameof(deserialize));
        }

        Task<T> ISerializer<T>.Deserialize(Stream stream)
        {
            return _deserialize(stream);
        }

        async Task ISerializer<T>.Serialize(Stream stream, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await item.Serialize(stream).ConfigureAwait(false);
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DelegateSerializer<T>
        : ISerializer<T>
    {
        private readonly Func<Stream, Task<T>> _deserialize;
        private readonly Func<Stream, T, Task> _serialize;

        public DelegateSerializer(
            Func<Stream, Task<T>> deserialize,
            Func<Stream, T, Task> serialize)
        {
            _deserialize =
                deserialize.Require(nameof(deserialize));

            _serialize =
                serialize.Require(nameof(serialize));
        }

        Task<T> ISerializer<T>.Deserialize(
            Stream stream)
        {
            stream.Require(nameof(stream));

            return _deserialize(stream);
        }

        Task ISerializer<T>.Serialize(
            Stream stream,
            T item)
        {
            stream.Require(nameof(stream));

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return _serialize(stream, item);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class VarintCollectionSerializer<T>
        : ICollectionSerializer<T>
    {
        private readonly ISerializer<T> _itemSerializer;

        public VarintCollectionSerializer(
            ISerializer<T> itemSerializer)
        {
            _itemSerializer =
                itemSerializer.Require(nameof(itemSerializer));
        }

        Task<IEnumerable<T>> ICollectionSerializer<T>.Deserialize(
            Stream stream)
        {
            stream.Require(nameof(stream));

            return Deserialize();

            async Task<IEnumerable<T>> Deserialize()
            {
                var items = new List<T>();

                int count = await Varint.Parse(stream).ConfigureAwait(false);
                for (int index = 0; index < count; index++)
                    items.Add(await _itemSerializer.Deserialize(stream).ConfigureAwait(false));

                return items;
            }
        }

        Task ICollectionSerializer<T>.Serialize(
            Stream stream,
            IEnumerable<T> items)
        {
            items.Require(nameof(items));

            return Serialize();

            async Task Serialize()
            {
                items = items.ToList();

                byte[] header = Varint.GetBytes(items.Count()).ToArray();
                await stream.WriteAsync(header, 0, header.Length).ConfigureAwait(false);

                foreach (T item in items)
                    await _itemSerializer.Serialize(stream, item).ConfigureAwait(false);
            }
        }
    }
}

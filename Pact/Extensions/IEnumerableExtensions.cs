using System;
using System.Collections.Generic;

namespace Pact.Extensions.Enumerable
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> @delegate)
        {
            if (@delegate == null)
                throw new ArgumentNullException(nameof(@delegate));

            if (items == null)
                return;

            foreach (T item in items)
                @delegate(item);
        }
    }
}

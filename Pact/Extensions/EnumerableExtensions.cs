using System;
using System.Collections.Generic;

using Pact.Extensions.Contract;

namespace Pact.Extensions.Enumerable
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(
            this IEnumerable<T> items,
            Action<T> @delegate)
        {
            items.Require(nameof(items));
            @delegate.Require(nameof(@delegate));

            foreach (T item in items)
                @delegate(item);
        }
    }
}

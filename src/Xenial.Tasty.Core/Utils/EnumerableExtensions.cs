using System;
using System.Collections.Generic;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Utils
{
    internal static class EnumerableExtensions
    {
        internal static TestOutcome MinOrDefault<TSource>(this IEnumerable<TSource> sequence, Func<TSource, TestOutcome> selector)
        {
            if (sequence.Any())
            {
                return sequence.Min(selector);
            }
            else
            {
                return default;
            }
        }
    }
}

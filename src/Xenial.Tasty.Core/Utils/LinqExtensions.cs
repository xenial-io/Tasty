using System;
using System.Collections.Generic;
using System.Linq;

namespace Xenial.Delicious.Utils
{
    internal static class LinqExtensions
    {
        internal static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
            => source
                .Select(selector)
                .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);

        internal static string AsDuration(this TimeSpan time)
           => $"[{time:hh\\:mm\\:ss\\.ffff}]";
    }
}

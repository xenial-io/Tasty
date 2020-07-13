using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Utils
{
    public static class EnumerableExtensions
    {
        public static TestOutcome MinOrDefault<TSource>(this IEnumerable<TSource> sequence, Func<TSource, TestOutcome> selector)
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

    /// <summary>
    /// CastExtentions for fluent casting
    /// </summary>
    public static class CastExtentions
    {
        /// <summary>
        /// Tries to cast the specified object to the type of T.<br/>
        /// Returns default if the value is not of this type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A casted object to the type of T</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static T? As<T>(this object obj)
            where T : class
            => obj is T item ? item : default;

        /// <summary>
        /// Tries to hard cast the specified object to the type of T.<br/>
        /// Returns default if the value is not of this type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A casted object to the type of T</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static T Cast<T>(this object obj)
            => (T)obj;
    }
}

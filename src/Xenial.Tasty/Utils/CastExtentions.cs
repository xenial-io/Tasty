using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xenial.Delicious.Utils
{
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
        public static T As<T>(this object obj)
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

using System;

namespace Xenial.Delicious.Utils
{
    internal static class Actions
    {
        internal static FinallyAction Finally(Action action, Action @finally)
            => new FinallyAction(action, @finally);

        internal static TryAction Try(Action action, Action @try)
            => new TryAction(action, @try);

        internal static TryAction<T> Try<T>(Action action, Action<T> @try)
            where T : Exception
            => new TryAction<T>(action, @try);

        internal static TryFinallyAction<T> Try<T>(Action action, Action<T>? @try = null, Action? @finally = null)
            where T : Exception
            => new TryFinallyAction<T>(action, @try, @finally);

        internal static TryFinallyAction Try(Action action, Action? @try = null, Action? @finally = null)
            => new TryFinallyAction(action, @try, @finally);
    }

    internal struct FinallyAction : IDisposable
    {
        private readonly Action finallyAction;
        /// <summary>
        /// Initializes a new instance of the <see cref="FinallyAction" /> struct.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="finally">The disposable.</param>
        internal FinallyAction(Action action, Action @finally)
        {
            action();
            finallyAction = @finally;
        }

        void IDisposable.Dispose()
            => finallyAction.Invoke();
    }

    internal struct TryAction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is by design")]
        internal TryAction(Action action, Action? @catch = null)
        {
            try
            {
                action();
            }
            catch
            {
                @catch?.Invoke();
            }
        }
    }

    internal struct TryAction<T> where T : Exception
    {
        internal TryAction(Action action, Action<T>? @catch = null)
        {
            try
            {
                action();
            }
            catch (T ex)
            {
                @catch?.Invoke(ex);
            }
        }
    }

    internal struct TryFinallyAction<T> where T : Exception
    {
        internal TryFinallyAction(Action action, Action<T>? @catch = null, Action? @finally = null)
        {
            try
            {
                action();
            }
            catch (T ex)
            {
                @catch?.Invoke(ex);
            }
            finally
            {
                @finally?.Invoke();
            }
        }
    }

    internal struct TryFinallyAction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is by design")]
        internal TryFinallyAction(Action action, Action? @catch = null, Action? @finally = null)
        {
            try
            {
                action();
            }
            catch
            {
                @catch?.Invoke();
            }
            finally
            {
                @finally?.Invoke();
            }
        }
    }
}

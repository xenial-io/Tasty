using System;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ResetConsoleColorMiddleware
    {
        public static TestExecutor UseResetConsoleColor(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next().ConfigureAwait(false);
                }
                finally
                {
                    Console.ResetColor();
                }
            });
    }
}

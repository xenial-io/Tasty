using System;
using System.IO;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ClearConsoleMiddleware
    {
        public static TestExecutor UseClearConsole(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.Scope.ClearBeforeRun)
                    {
                        try
                        {
                            Console.Clear();
                        }
                        catch (IOException) { /* Handle is invalid */}
                    }
                }
                finally
                {
                    await next().ConfigureAwait(false);
                }
            });
    }
}

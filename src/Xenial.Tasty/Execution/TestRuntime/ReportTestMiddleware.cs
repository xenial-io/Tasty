using System;
using System.Linq;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static partial class ReportTestMiddleware
    {
        public static TestExecutor UseRemoteClearConsole(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.Scope.ClearBeforeRun && context.Remote != null)
                    {
                        await context.Remote.ClearConsole();
                    }
                }
                finally
                {
                    await next();
                }
            });
    }
}

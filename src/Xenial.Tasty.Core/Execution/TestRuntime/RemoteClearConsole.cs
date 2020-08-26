using System;
using System.Linq;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class RemoteClearConsole
    {
        public static TestExecutor UseRemoteClearConsole(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.Scope.ClearBeforeRun && context.Remote != null)
                    {
                        await context.Remote.ClearConsole().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await next().ConfigureAwait(false);
                }
            });
    }
}

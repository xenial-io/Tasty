namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ResetRemoteConsoleColorMiddleware
    {
        public static TestExecutor UseResetRemoteConsoleColor(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    if (context.Scope.ClearBeforeRun && context.Remote != null)
                    {
                        await context.Remote.ResetColor();
                    }
                }
            });
    }
}

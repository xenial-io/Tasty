namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class DetectInteractiveRunMiddleware
    {
        public static TestExecutor UseInteractiveRunDetection(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    context.IsInteractive = await context.Scope.IsInteractiveRunHook().ConfigureAwait(false);
                }
                finally
                {
                    await next().ConfigureAwait(false);
                }
            });
    }
}

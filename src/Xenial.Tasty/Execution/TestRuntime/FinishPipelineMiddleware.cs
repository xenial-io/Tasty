namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class FinishPipelineMiddleware
    {
        public static TestExecutor UseFinishPipeline(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next();
                }
                finally
                {
                    if (!context.IsInteractive)
                    {
                        context.IsFinished = true;
                    }
                }
            });
    }
}

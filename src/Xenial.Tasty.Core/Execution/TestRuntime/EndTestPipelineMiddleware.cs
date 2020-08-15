
using Xenial.Delicious.Commands;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class EndTestPipelineMiddleware
    {
        public static TestExecutor UseEndTestPipeline(this TestExecutor executor)
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
                        context.EndPipeLine = true;
                    }
                    if (context.EndPipeLine && context.Remote != null)
                    {
                        await context.Remote.SignalEndTestPipeline();
                    }
                }
            });
    }
}

using System.Diagnostics;

namespace Xenial.Delicious.Execution.TestGroupMiddleware
{
    public static class StopWatchGroupMiddleware
    {
        public static TestExecutor UseTestGroupStopwatch(this TestExecutor executor)
            => executor.UseGroup(async (context, next) =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await next();
                }
                finally
                {
                    sw.Stop();

                    context.CurrentGroup.Duration = sw.Elapsed;
                }
            });
    }
}

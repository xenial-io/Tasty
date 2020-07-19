using System;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class RunCommandMiddleware
    {
        public static TestExecutor UseRunCommands(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.CurrentCommand != null)
                    {
                        await context.CurrentCommand.Command(context);
                    }
                }
                finally
                {
                    await next();
                }
            });
    }
}

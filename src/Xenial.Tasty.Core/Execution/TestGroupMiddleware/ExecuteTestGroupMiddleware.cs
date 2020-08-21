using System;
using System.Linq;

namespace Xenial.Delicious.Execution.TestGroupMiddleware
{
    public static class ExecuteTestGroupMiddleware
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This grabs into user code")]
        public static TestExecutor UseTestGroupExecutor(this TestExecutor executor)
            => executor.UseGroup(async (context, next) =>
            {
                try
                {
                    var result = await context.CurrentGroup.Executor().ConfigureAwait(false);

                    context.CurrentGroup.TestOutcome = result
                        ? Metadata.TestOutcome.Success
                        : Metadata.TestOutcome.Failed;

                    if (result)
                    {
                        await next().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    context.CurrentGroup.Exception = ex;
                    context.CurrentGroup.TestOutcome = Metadata.TestOutcome.Failed;
                }
            });
    }
}

using System;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Execution.TestMiddleware
{
    public static class IgnoreTestMiddleware
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This grabs into user code")]
        public static TestExecutor UseIgnoreTestExecutor(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                try
                {
                    if (context.CurrentCase.IsIgnored != null)
                    {
                        var result = context.CurrentCase.IsIgnored();
                        if (result.HasValue)
                        {
                            if (result.Value)
                            {
                                context.CurrentCase.TestOutcome = TestOutcome.Ignored;
                            }
                            else
                            {
                                await next().ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        await next().ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    context.CurrentCase.Exception = exception;
                    context.CurrentCase.TestOutcome = TestOutcome.Failed;
                }
            });
    }
}

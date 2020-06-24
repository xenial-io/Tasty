using System;

namespace Xenial.Delicious.Execution.TestGroupMiddleware
{
    public static class CurrentScopeGroupMiddleware
    {
        public static TestExecutor UseTestGroupScope(this TestExecutor executor)
            => executor.UseGroup(async (context, next) =>
            {
                context.CurrentScope.CurrentGroup = context.CurrentGroup;
                await next();
            });
    }
}

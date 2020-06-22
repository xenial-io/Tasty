using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Execution.Middleware
{
    public static class BeforeEachTestMiddleware
    {
        public static TestExecutor UseBeforeEachTest(this TestExecutor executor)
            => executor.Use(async (context, next) =>
            {
                foreach (var hook in
                    context.CurrentCase.Group?.BeforeEachHooks
                    ?? context.CurrentScope.BeforeEachHooks
                    )
                {
                    var hookResult = await hook.Executor.Invoke();
                    if (!hookResult)
                    {
                        return;
                    }
                }
                await next();
            });
    }
}
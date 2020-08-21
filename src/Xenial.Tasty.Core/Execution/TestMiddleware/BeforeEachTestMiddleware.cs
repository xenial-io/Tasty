namespace Xenial.Delicious.Execution.TestMiddleware
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
                    var hookResult = await hook.Executor.Invoke().ConfigureAwait(false);
                    if (!hookResult)
                    {
                        return;
                    }
                }
                await next().ConfigureAwait(false);
            });
    }
}

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class RemoteStreamDisposalMiddleware
    {
        public static TestExecutor UseRemoteDisposal(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    await next().ConfigureAwait(false);
                }
                finally
                {
                    if (!context.IsInteractive)
                    {
                        context.RemoteStream?.Dispose();
                        context.Remote?.Dispose();
                    }
                }
            });
    }
}

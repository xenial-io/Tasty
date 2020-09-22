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
#if NET5
                        if (context.RemoteStream != null)
                        {
                            await context.RemoteStream.DisposeAsync().ConfigureAwait(false);
                        }
#else
                        context.RemoteStream?.Dispose();
#endif
                        context.Remote?.Dispose();
                    }
                }
            });
    }
}

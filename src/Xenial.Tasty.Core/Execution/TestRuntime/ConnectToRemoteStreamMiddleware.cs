using System.Threading;

namespace Xenial.Delicious.Execution.TestRuntime
{
    public static class ConnectToRemoteStreamMiddleware
    {
        public static TestExecutor UseRemote(this TestExecutor executor)
            => executor.UseRuntime(async (context, next) =>
            {
                try
                {
                    if (context.RemoteConnectionString != null
                        && context.RemoteStream == null
                        && context.Remote == null
                    )
                    {
                        if (context.Scope.ClientTransportStreamFactories.TryGetValue(context.RemoteConnectionString.Scheme, out var remoteStreamFactoryFunctor))
                        {
                            var cts = new CancellationTokenSource();
                            try
                            {
                                cts.CancelAfter(10000); //TODO: Make configurable
                                var remoteStreamFactory = await remoteStreamFactoryFunctor(context.RemoteConnectionString, cts.Token).ConfigureAwait(false);
                                if (remoteStreamFactory != null)
                                {
                                    context.RemoteStream = await remoteStreamFactory.Invoke().ConfigureAwait(false);
                                    context.Remote = await context.Scope.ConnectToRemoteRunHook(context.Scope, context.RemoteStream).ConfigureAwait(false);
                                }
                            }
                            finally
                            {
                                cts.Dispose();
                            }
                        }
                    }
                }
                finally
                {
                    await next().ConfigureAwait(false);
                }
            });
    }
}

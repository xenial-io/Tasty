using System.Diagnostics;
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
                    if (context.IsInteractive)
                    {
                        foreach (var remoteStreamFactoryFunctor in context.Scope.TransportStreamFactories)
                        {
                            var cts = new CancellationTokenSource();
                            try
                            {
                                cts.CancelAfter(10000); //TODO: Make configurable
                                var remoteStreamFactory = await remoteStreamFactoryFunctor(cts.Token);
                                if (remoteStreamFactory != null)
                                {
                                    context.RemoteStream = await remoteStreamFactory.Invoke();
                                    context.Remote = await context.Scope.ConnectToRemoteRunHook(context.Scope, context.RemoteStream);

                                    break;
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
                    await next();
                }
            });
    }
}

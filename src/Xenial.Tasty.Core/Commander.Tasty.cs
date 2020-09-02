using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using StreamJsonRpc;

using Xenial.Delicious.Plugins;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Remote;
using Xenial.Delicious.Reporters;

using static Xenial.Delicious.Utils.PromiseHelper;

namespace Xenial.Delicious.Commanders
{
    public class TastyCommander : IDisposable
    {
        private Dictionary<string, TransportStreamFactoryFunctor> TransportStreamFactories { get; } = new Dictionary<string, TransportStreamFactoryFunctor>();
        private readonly List<AsyncTestReporter> reporters = new List<AsyncTestReporter>();
        private readonly List<AsyncTestSummaryReporter> summaryReporters = new List<AsyncTestSummaryReporter>();
        private readonly IList<IDisposable> disposables = new List<IDisposable>();
        private TastyServer? tastyServer;
        private JsonRpc? jsonRpc;
        private readonly ConcurrentQueue<TestCaseResult> queue = new ConcurrentQueue<TestCaseResult>();
        private bool pluginsLoaded;
        public bool LoadPlugins { get; set; } = true;
        public bool IsDisposed { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsConnected => tastyServer is object;

        public TastyCommander RegisterReporter(AsyncTestReporter reporter)
        {
            reporters.Add(reporter);
            return this;
        }

        public TastyCommander RegisterReporter(AsyncTestSummaryReporter reporter)
        {
            summaryReporters.Add(reporter);
            return this;
        }

        public TastyCommander RegisterTransport(string protocol, TransportStreamFactoryFunctor transportStreamFactory)
        {
            _ = transportStreamFactory ?? throw new ArgumentNullException(nameof(transportStreamFactory));
            TransportStreamFactories[protocol] = transportStreamFactory;
            return this;
        }

        protected async Task TryLoadPlugins()
        {
            if (pluginsLoaded)
            {
                return;
            }

            if (LoadPlugins)
            {
                var pluginLoader = new CommanderPluginLoader();
                await pluginLoader.LoadPlugins(this).ConfigureAwait(false);
                pluginsLoaded = true;
            }
        }

        private async Task Report(TestCaseResult testCaseResult)
        {
            queue.Enqueue(testCaseResult);
            foreach (var reporter in reporters)
            {
                await reporter.Invoke(testCaseResult).ConfigureAwait(false);
            }
        }

        private async Task ReportSummary(IEnumerable<TestCaseResult> testCases)
        {
            foreach (var reporter in summaryReporters)
            {
                await reporter.Invoke(testCases).ConfigureAwait(false);
            }
        }

        internal async Task<TastyServer?> ConnectToRemote(System.Uri connectionString, CancellationToken cancellationToken = default)
        {
            if (tastyServer is null && TransportStreamFactories.TryGetValue(connectionString.Scheme, out var factory))
            {
                var result = await factory(connectionString, cancellationToken).ConfigureAwait(false);
                var streamTask = result();

                disposables.Add(streamTask);
                var stream = await streamTask.ConfigureAwait(false);

                tastyServer = new TastyServer();

                tastyServer.RegisterReporter(Report);
                tastyServer.RegisterReporter(ReportSummary);
                tastyServer.EndTestPipelineSignaled += () => IsRunning = false;

                jsonRpc = JsonRpc.Attach(stream, tastyServer);
                disposables.Add(jsonRpc);
                IsRunning = true;
                return tastyServer;
            }
            return null;
        }

        public virtual IAsyncEnumerable<TestCaseResult> Run(CancellationToken cancellationToken = default)
            => WaitForResults(cancellationToken);

        protected async IAsyncEnumerable<TestCaseResult> WaitForResults([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (tastyServer is null)
            {
                yield break;
            }

            await Task.CompletedTask.ConfigureAwait(false);

            while (IsRunning)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (queue.Count > 0)
                {
                    if (queue.TryDequeue(out var testCaseResult))
                    {
                        yield return testCaseResult;
                    }
                }
            }
        }

        internal Task<IList<SerializableTastyCommand>> ListCommands(CancellationToken token = default)
            => Promise<IList<SerializableTastyCommand>>((resolve, reject) =>
            {
                if (tastyServer is object)
                {
                    var cts = new CancellationTokenSource();

                    //TODO: this may cause a memory leak...
                    disposables.Add(cts);

                    //TODO: Make this configurable
                    cts.CancelAfter(10000);

                    cts.Token.CombineWith(token)
                        .Token.Register(() =>
                        {
                            try
                            {
                                reject(cts.Token);
                            }
                            catch (ObjectDisposedException) { }
                        });

                    tastyServer.CommandsRegistered = (c) =>
                    {
                        cts.Dispose();
                        tastyServer.CommandsRegistered = null;
                        resolve(c);
                    };
                }
                else
                {
                    reject(token);
                }
            });

        public Action? EndTestPipelineSignaled
        {
            get => tastyServer?.EndTestPipelineSignaled;
            set
            {
                if (tastyServer != null)
                {
                    tastyServer.EndTestPipelineSignaled = value;
                }
            }
        }

        public Action? TestPipelineCompletedSignaled
        {
            get => tastyServer?.TestPipelineCompletedSignaled;
            set
            {
                if (tastyServer != null)
                {
                    tastyServer.TestPipelineCompletedSignaled = value;
                }
            }
        }

        public Task DoRequestCancellation()
        {
            if (tastyServer != null)
            {
                tastyServer?.DoRequestCancellation();
            }
            return Task.CompletedTask;
        }

        public Task DoExecuteCommand(ExecuteCommandEventArgs executeCommandEventArgs)
        {
            var server = tastyServer;
            if (server != null)
            {
                return server.DoExecuteCommand(executeCommandEventArgs);
            }
            return Task.CompletedTask;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design")]
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    foreach (var disposable in disposables)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    jsonRpc = null;
                    tastyServer = null;
                }

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

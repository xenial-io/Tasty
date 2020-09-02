using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using StreamJsonRpc;

using Xenial.Delicious.Plugins;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Remote;
using Xenial.Delicious.Reporters;

using static SimpleExec.Command;
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

        internal async Task<int> BuildProject(string csProjFileName, IProgress<(string line, bool isRunning, int exitCode)>? progress = null, CancellationToken cancellationToken = default)
        {
            await TryLoadPlugins().ConfigureAwait(false);

            return await Task.Run(async () =>
            {
                var buildTask = BuildAsync(csProjFileName, cancellationToken);

                await foreach (var (line, hasStopped, exitCode) in buildTask)
                {
                    progress?.Report((line, hasStopped, exitCode));
                    if (hasStopped)
                    {
                        return exitCode;
                    }
                }
                return 0;
            }).ConfigureAwait(false);
        }

        private static async IAsyncEnumerable<(string line, bool hasStopped, int exitCode)> BuildAsync(
            string csProjFileName,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", $"build \"{csProjFileName}\" -f netcoreapp3.1")
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,

                    //On windows dotnet core seams to set the codepage to 850
                    //see: https://github.com/dotnet/runtime/issues/17849#issuecomment-353612399
                    StandardOutputEncoding = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? Encoding.GetEncoding(850) //DOS-Latin-1
                        : Encoding.Default,
                }
            };

            proc.Start();

            using var reader = proc.StandardOutput;
            while (!reader.EndOfStream)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    proc.Close();
                    yield break;
                }
                var line = await reader.ReadLineAsync().ConfigureAwait(false) ?? string.Empty;
                yield return (line, false, 0);
            }

            var exitCode = await proc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            yield return (string.Empty, true, exitCode);
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

        internal async Task<Task> ConnectToRemote(string csProjFileName, CancellationToken cancellationToken = default)
        {
            // TODO: write a connection string builder once we introduce the next transport
            var connectionId = $"TASTY_{Guid.NewGuid()}";
            var connectionString = new Uri($"{Uri.UriSchemeNetPipe}://localhost/{connectionId}");

            var remote = ConnectToRemote(connectionString, cancellationToken).ConfigureAwait(false);

            var remoteTask = ReadAsync("dotnet",
                $"run -p \"{csProjFileName}\" -f netcoreapp3.1 --no-restore --no-build",
                noEcho: true,
                configureEnvironment: (env) =>
                {
                    env.Add(EnvironmentVariables.InteractiveMode, "true");
                    env.Add(EnvironmentVariables.TastyConnectionString, connectionString.ToString());
                }
            );

            await remote;

            return remoteTask;
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

using System;
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
    public sealed class TastyCommander : IDisposable
    {
        public bool LoadPlugins { get; set; } = true;

        internal Dictionary<string, TransportStreamFactoryFunctor> TransportStreamFactories { get; } = new Dictionary<string, TransportStreamFactoryFunctor>();

        private readonly List<AsyncTestReporter> reporters = new List<AsyncTestReporter>();
        private readonly List<AsyncTestSummaryReporter> summaryReporters = new List<AsyncTestSummaryReporter>();
        private readonly IList<IDisposable> disposables = new List<IDisposable>();
        private TastyServer? tastyServer;
        private JsonRpc? jsonRpc;

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

        internal async Task<int> BuildProject(string csProjFileName, IProgress<(string line, bool isRunning, int exitCode)>? progress = null, CancellationToken cancellationToken = default)
        {
            _ = this;

            if (LoadPlugins)
            {
                var pluginLoader = new CommanderPluginLoader();
                await pluginLoader.LoadPlugins(this).ConfigureAwait(false);
            }

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

        private async Task Report(TestCaseResult testCase)
        {
            foreach (var reporter in reporters)
            {
                await reporter.Invoke(testCase).ConfigureAwait(false);
            }
        }

        private async Task ReportSummary(IEnumerable<TestCaseResult> testCases)
        {
            foreach (var reporter in summaryReporters)
            {
                await reporter.Invoke(testCases).ConfigureAwait(false);
            }
        }

        internal async Task<Task> ConnectToRemote(string csProjFileName, CancellationToken cancellationToken = default)
        {
            // TODO: write a connection string builder once we introduce the next transport
            var connectionId = $"TASTY_{Guid.NewGuid()}";
            var connectionString = new Uri($"{Uri.UriSchemeNetPipe}://localhost/{connectionId}");

            //TODO: we should not rely on default scope here
            if (TransportStreamFactories.TryGetValue(connectionString.Scheme, out var factory))
            {
                var result = await factory(connectionString, cancellationToken).ConfigureAwait(false);
                var streamTask = result();

                var remoteTask = ReadAsync("dotnet",
                    $"run -p \"{csProjFileName}\" -f netcoreapp3.1 --no-restore --no-build",
                    noEcho: true,
                    configureEnvironment: (env) =>
                    {
                        env.Add(EnvironmentVariables.InteractiveMode, "true");
                        env.Add(EnvironmentVariables.TastyConnectionString, connectionString.ToString());
                    }
                );

                disposables.Add(streamTask);
                disposables.Add(remoteTask);
                var stream = await streamTask.ConfigureAwait(false);

                tastyServer = new TastyServer();

                tastyServer.RegisterReporter(Report);
                tastyServer.RegisterReporter(ReportSummary);

                jsonRpc = JsonRpc.Attach(stream, tastyServer);
                disposables.Add(jsonRpc);
                return remoteTask;
            }

            return Task.CompletedTask;
        }

        internal Task<IList<SerializableTastyCommand>> ListCommands(CancellationToken token = default)
            => Promise<IList<SerializableTastyCommand>>((resolve, reject) =>
            {
                //TODO: Make a guard method
                if (tastyServer != null)
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design")]
        public void Dispose()
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

        public Task DoExecuteCommand(ExecuteCommandEventArgs executeCommandEventArgs)
        {
            var server = tastyServer;
            if (server != null)
            {
                return server.DoExecuteCommand(executeCommandEventArgs);
            }
            return Task.CompletedTask;
        }
    }
}

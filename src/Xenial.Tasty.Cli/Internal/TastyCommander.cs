using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using StreamJsonRpc;

using Xenial.Delicious.Protocols;
using Xenial.Delicious.Remote;
using static SimpleExec.Command;
using static Xenial.Delicious.Utils.PromiseHelper;

namespace Xenial.Delicious.Cli.Internal
{
    internal class TastyCommander : IDisposable
    {
        private readonly List<AsyncRemoteTestReporter> Reporters = new List<AsyncRemoteTestReporter>();
        private readonly List<AsyncRemoteTestSummaryReporter> SummaryReporters = new List<AsyncRemoteTestSummaryReporter>();

        public TastyCommander RegisterReporter(AsyncRemoteTestReporter reporter)
        {
            Reporters.Add(reporter);
            return this;
        }

        public TastyCommander RegisterReporter(AsyncRemoteTestSummaryReporter reporter)
        {
            SummaryReporters.Add(reporter);
            return this;
        }

        IList<IDisposable> Disposables = new List<IDisposable>();

        TastyServer? _TastyServer;
        JsonRpc? _JsonRpc;

        internal async Task<int> BuildProject(string csProjFileName, IProgress<(string line, bool isRunning, int exitCode)>? progress = null, CancellationToken cancellationToken = default)
        {
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
            });
        }

        static async IAsyncEnumerable<(string line, bool hasStopped, int exitCode)> BuildAsync(string csProjFileName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

            using StreamReader reader = proc.StandardOutput;
            while (!reader.EndOfStream)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    proc.Close();
                    yield break;
                }
                var line = await reader.ReadLineAsync() ?? string.Empty;
                yield return (line, false, 0);
            }

            var exitCode = await proc.WaitForExitAsync(cancellationToken);
            yield return (string.Empty, true, exitCode);
        }

        async Task Report(SerializableTestCase @case)
        {
            foreach (var reporter in Reporters)
            {
                await reporter.Invoke(@case);
            }
        }

        async Task ReportSummary(IEnumerable<SerializableTestCase> @cases)
        {
            foreach (var reporter in SummaryReporters)
            {
                await reporter.Invoke(@cases);
            }
        }

        internal async Task<Task> ConnectToRemote(string csProjFileName, CancellationToken cancellationToken = default)
        {
            var connectionId = $"TASTY_{Guid.NewGuid()}";

            var stream = new NamedPipeServerStream(
                connectionId,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous
            );

            Disposables.Add(stream);
            var connectionTask = stream.WaitForConnectionAsync(cancellationToken);

            var remoteTask = ReadAsync("dotnet",
                $"run -p \"{csProjFileName}\" -f netcoreapp3.1 --no-restore --no-build",
                noEcho: true,
                configureEnvironment: (env) =>
                {
                    env.Add(EnvironmentVariables.InteractiveMode, "true");
                    env.Add(EnvironmentVariables.InteractiveConnectionType, "NamedPipes");
                    env.Add(EnvironmentVariables.InteractiveConnectionId, connectionId);
                }
            );

            Disposables.Add(remoteTask);

            await connectionTask;

            _TastyServer = new TastyServer();

            _TastyServer.RegisterReporter(Report);
            _TastyServer.RegisterReporter(ReportSummary);

            _JsonRpc = JsonRpc.Attach(stream, _TastyServer);
            Disposables.Add(_JsonRpc);
            return remoteTask;
        }

        internal Task<IList<SerializableTastyCommand>> ListCommands(CancellationToken token = default)
            => Promise<IList<SerializableTastyCommand>>((resolve, reject) =>
            {
                //TODO: Make a guard method
                if (_TastyServer != null)
                {
                    var cts = new CancellationTokenSource();

                    //TODO: this may cause a memory leak...
                    Disposables.Add(cts);

                    //TODO: Make this configurable
                    cts.CancelAfter(10000);

                    cts.Token.CombineWith(token)
                        .Token.Register(() => reject(cts.Token));

                    _TastyServer.CommandsRegistered = (c) =>
                    {
                        cts.Dispose();
                        _TastyServer.CommandsRegistered = null;
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
            get => _TastyServer?.EndTestPipelineSignaled;
            set
            {
                if (_TastyServer != null)
                {
                    _TastyServer.EndTestPipelineSignaled = value;
                }
            }
        }

        public Action? TestPipelineCompletedSignaled
        {
            get => _TastyServer?.TestPipelineCompletedSignaled;
            set
            {
                if (_TastyServer != null)
                {
                    _TastyServer.TestPipelineCompletedSignaled = value;
                }
            }
        }

        public Task DoRequestCancellation()
        {
            if (_TastyServer != null)
            {
                _TastyServer?.DoRequestCancellation();
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var disposable in Disposables)
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
            _JsonRpc = null;
            _TastyServer = null;
        }

        public async Task DoExecuteCommand(ExecuteCommandEventArgs executeCommandEventArgs)
        {
            //TODO: Make a guard method
            if (_TastyServer != null)
            {
                await _TastyServer.DoExecuteCommand(executeCommandEventArgs);
            }
        }
    }
}

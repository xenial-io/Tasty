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

using static SimpleExec.Command;

namespace Xenial.Delicious.Cli.Internal
{
    internal class TastyCommander : IDisposable
    {
        IList<IDisposable> Disposables = new List<IDisposable>();

        TastyServer? _TastyServer;
        JsonRpc? _JsonRpc;

        public async Task<int> BuildProject(string csProjFileName, IProgress<(string line, bool isRunning, int exitCode)> progress, CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () =>
            {
                var buildTask = BuildAsync(csProjFileName, cancellationToken);

                await foreach (var (line, hasStopped, exitCode) in buildTask)
                {
                    progress.Report((line, hasStopped, exitCode));
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

        public async Task ConnectToRemote(string csProjFileName, CancellationToken token = default)
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
            var connectionTask = stream.WaitForConnectionAsync(token);

            var remoteTask = ReadAsync("dotnet",
                $"run -p \"{csProjFileName}\" -f netcoreapp3.1 --no-restore --no-build",
                noEcho: true,
                configureEnvironment: (env) =>
                {
                    env.Add("TASTY_INTERACTIVE", "true");
                    env.Add("TASTY_INTERACTIVE_CON_TYPE", "NamedPipes");
                    env.Add("TASTY_INTERACTIVE_CON_ID", connectionId);
                }
            );

            await connectionTask;
            _TastyServer = new TastyServer();
            _JsonRpc = JsonRpc.Attach(stream, _TastyServer);
            Disposables.Add(_JsonRpc);
        }

        public void Dispose()
        {
            foreach(var disposable in Disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            _JsonRpc = null;
            _TastyServer = null;
        }
    }
}

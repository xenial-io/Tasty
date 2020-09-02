using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using Xenial.Delicious.Remote;

using static SimpleExec.Command;

namespace Xenial.Delicious.Commanders
{
    public class TastyProcessCommander : TastyCommander
    {
        public Uri ConnectionString { get; }

        public TastyProcessCommander(Uri connectionString)
            => ConnectionString = connectionString;

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
    }
}

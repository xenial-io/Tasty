using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using static SimpleExec.Command;

namespace Xenial.Delicious.Cli.Internal
{
    internal class TastyCommander
    {
        public async Task<int> BuildProject(string csProjFileName, IProgress<(string line, bool isRunning, int exitCode)> progress, CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () =>
            {
                var buildTask = BuildAsync(csProjFileName, cancellationToken);

                await foreach (var (line, isRunning, exitCode) in buildTask)
                {
                    progress.Report((line, isRunning, exitCode));
                    if (!isRunning)
                    {
                        return exitCode;
                    }
                }
                return 0;
            });
        }

        public static async IAsyncEnumerable<(string, bool, int)> BuildAsync(string csProjFileName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", $"build \"{csProjFileName}\" -f netcoreapp3.1")
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
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
                yield return (line, true, 0);
            }

            var exitCode = await proc.WaitForExitAsync(cancellationToken);
            yield return (string.Empty, true, exitCode);
        }
    }
}

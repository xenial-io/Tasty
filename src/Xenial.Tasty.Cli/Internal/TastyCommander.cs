using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

namespace Xenial.Delicious.Cli.Internal
{
    internal class TastyCommander
    {
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

        public static async IAsyncEnumerable<(string line, bool hasStopped, int exitCode)> BuildAsync(string csProjFileName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
    }
}

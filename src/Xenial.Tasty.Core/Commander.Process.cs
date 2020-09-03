using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using Xenial.Delicious.FeatureDetection;
using Xenial.Delicious.Remote;

namespace Xenial.Delicious.Commanders
{
    internal static class ProcessStartInfoHelper
    {
        public static ProcessStartInfo Create(
            string name,
            string args,
            string? workingDirectory = null,
            bool captureOutput = true,
            string? windowsName = null,
            string? windowsArgs = null,
            Action<IDictionary<string, string>>? configureEnvironment = null)
        {
            var isWindows = FeatureDetector.IsWindows();

            var startInfo = new ProcessStartInfo
            {
                FileName = isWindows ? (windowsName ?? name) : name,
                Arguments = isWindows ? (windowsArgs ?? args) : args,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardError = captureOutput,
                RedirectStandardOutput = captureOutput,
                //On windows dotnet core seams to set the codepage to 850
                //see: https://github.com/dotnet/runtime/issues/17849#issuecomment-353612399
                StandardOutputEncoding = isWindows
                    ? Encoding.GetEncoding(850) //DOS-Latin-1
                    : Encoding.Default,
            };

            configureEnvironment?.Invoke(startInfo.Environment);

            return startInfo;
        }

        public static async IAsyncEnumerable<(string line, bool isError, int? exitCode)> RunAsync(this Process process, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using (process)
            {
                var queue = new ConcurrentQueue<(string line, bool isError, int? exitCode)>();

                process.OutputDataReceived += (sender, eventArgs) => queue.Enqueue((eventArgs.Data, false, null));
                process.ErrorDataReceived += (sender, eventArgs) => queue.Enqueue((eventArgs.Data, true, null));
                if (process.Start())
                {
                    try
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    var processTask = process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                    while (!process.HasExited)
                    {
                        if (queue.TryDequeue(out var result))
                        {
                            yield return result;
                        }
                    }

                    while (queue.Count > 0)
                    {
                        if (queue.TryDequeue(out var result))
                        {
                            yield return result;
                        }
                    }

                    var exitCode = await processTask;

                    yield return (string.Empty, exitCode != 0, exitCode);
                }
            }
        }
    }

    public class TastyProcessCommander : TastyRemoteCommander
    {
        public IProgress<(string line, bool isError, int? exitCode)>? Progress { get; }

        public TastyProcessCommander(Uri connectionString, Func<ProcessStartInfo> processFactory, IProgress<(string line, bool isError, int? exitCode)>? progress = null)
            : this(connectionString, () => new Process
            {
                StartInfo = processFactory()
            }, progress)
        { }

        internal TastyProcessCommander(Uri connectionString, Func<Process> processFactory, IProgress<(string line, bool isError, int? exitCode)>? progress = null) : base(connectionString, (cancellationToken) =>
        {
            var process = processFactory();
            process.StartInfo.EnvironmentVariables[EnvironmentVariables.TastyConnectionString] = connectionString.ToString();

            if (progress is object)
            {
                process.OutputDataReceived += (sender, eventArgs) => progress.Report((eventArgs.Data, false, null));
                process.ErrorDataReceived += (sender, eventArgs) => progress.Report((eventArgs.Data, true, null));
            }

            process.EnableRaisingEvents = true;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process.WaitForExitAsync(cancellationToken);
        }) => Progress = progress;
    }
}

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using StreamJsonRpc;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Utils;

using static SimpleExec.Command;
using static Xenial.Delicious.Utils.Actions;

namespace Xenial.Tasty.Tool
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            var interactiveCommand = new Command("interactive")
            {
                Handler = CommandHandler.Create<string, CancellationToken>(Interactive)
            };
            interactiveCommand.AddAlias("i");
            var arg = new Option<string>("--project");
            arg.AddAlias("-p");
            interactiveCommand.Add(arg);
            rootCommand.Add(interactiveCommand);

            return await rootCommand.InvokeAsync(args);
        }

        static async Task<int> Interactive(string project, CancellationToken cancellationToken)
        {
            try
            {
                var path = Path.GetFullPath(project);
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    var directoryName = new DirectoryInfo(path).Name;
                    var csProjFileName = Path.Combine(path, $"{directoryName}.csproj");
                    if (File.Exists(csProjFileName))
                    {
                        Console.WriteLine(csProjFileName);

                        var connectionId = $"TASTY_{Guid.NewGuid()}";

                        using var stream = new NamedPipeServerStream(connectionId, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                        var connectionTask = stream.WaitForConnectionAsync(cancellationToken);
                        var remoteTask = ReadAsync("dotnet",
                            $"run -p \"{csProjFileName}\" -f netcoreapp3.1",
                            noEcho: true,
                            configureEnvironment: (env) =>
                            {
                                env.Add("TASTY_INTERACTIVE", "true");
                                env.Add("TASTY_INTERACTIVE_CON_TYPE", "NamedPipes");
                                env.Add("TASTY_INTERACTIVE_CON_ID", connectionId);
                            }
                        );

                        Console.WriteLine("Connecting. NamedPipeServerStream...");
                        await connectionTask;
                        var server = new TastyServer();
                        using var tastyServer = JsonRpc.Attach(stream, server);

                        Console.WriteLine("Connected. NamedPipeServerStream...");
                        try
                        {
                            var uiTask = Task.Run(async () =>
                            {
                                var commands = await Task.Run(async () => await WaitForCommands(server), cancellationToken);
                                await Task.Run(async () => await WaitForInput(commands, server), cancellationToken);
                            }, cancellationToken);

                            await Task.WhenAll(remoteTask, uiTask);
                        }
                        catch (TaskCanceledException)
                        {
                            return 0;
                        }
                        catch (SimpleExec.NonZeroExitCodeException e)
                        {
                            return e.ExitCode;
                        }
                    }
                }
                return 0;
            }
            catch (TaskCanceledException)
            {
                return 1;
            }
        }

        static Task<IList<SerializableTastyCommand>> WaitForCommands(TastyServer tastyServer)
        {
            var tcs = new TaskCompletionSource<IList<SerializableTastyCommand>>();
            var cts = new CancellationTokenSource();

            cts.CancelAfter(10000);
            cts.Token.Register(() => tcs.SetCanceled());

            tastyServer.CommandsRegistered = (c) =>
            {
                cts.Dispose();
                tcs.SetResult(c);
            };

            return tcs.Task;
        }

        static Task WaitForInput(IList<SerializableTastyCommand> commands, TastyServer tastyServer)
        {
            var cts = new CancellationTokenSource();

            return Task.Run(async () =>
            {
                Console.CancelKeyPress += (_, e) =>
                {
                    if (e.Cancel)
                    {
                        Console.WriteLine("Cancelling execution...");
                        cts.Cancel();
                        throw new TaskCanceledException(null, null, cts.Token);
                    }
                };

                tastyServer.FinishSignaled = () =>
                {
                    Console.WriteLine("Cancelling execution...");
                    cts.Cancel();
                };

                async Task<Func<Task>?> ReadInput()
                {
                    Console.WriteLine("Interactive Mode");
                    Console.WriteLine("================");

                    foreach (var c in commands)
                    {
                        Console.WriteLine($"{c.Name} - {c.Description}" + (c.IsDefault ? " (default)" : string.Empty));
                    }

                    Console.WriteLine("c - Cancel");
                    Console.WriteLine("================");

                    Task<ConsoleKeyInfo> ReadKey()
                    {
                        var keyTcs = new TaskCompletionSource<ConsoleKeyInfo>();
                        cts.Token.Register(() => keyTcs.SetCanceled());

                        _ = Task.Run(() =>
                        {
                            var info = Console.ReadKey(true);
                            keyTcs.SetResult(info);
                        });

                        return keyTcs.Task;
                    }

                    var info = await ReadKey();

                    var key = info.Key.ToString().ToLower();

                    var command = info.Key == ConsoleKey.Enter
                        ? commands.FirstOrDefault(c => c.IsDefault)
                        : commands.FirstOrDefault(c => c.Name.ToLowerInvariant() == key);

                    if (command != null)
                    {
                        Console.WriteLine($"Executing {command.Name} - {command.Description}");

                        return async () =>
                        {
                            await tastyServer.DoExecuteCommand(new ExecuteCommandEventArgs
                            {
                                CommandName = command.Name
                            });
                        };
                    }

                    if (info.Key == ConsoleKey.C)
                    {
                        await tastyServer.DoRequestCancellation();
                        cts.Cancel();
                        return null;
                    }

                    return async () => await ReadInput();
                }
                var action = await ReadInput();
                while (!cts.IsCancellationRequested && action != null)
                {
                    await action();
                    action = await ReadInput();
                }

            }, cts.Token);
        }
    }

    public class TastyServer
    {
        public event EventHandler<ExecuteCommandEventArgs>? ExecuteCommand;
        public event EventHandler? CancellationRequested;

        internal async Task DoExecuteCommand(ExecuteCommandEventArgs args)
        {
            ExecuteCommand?.Invoke(this, args);
            await Task.CompletedTask;
        }

        internal async Task DoRequestCancellation()
        {
            CancellationRequested?.Invoke(this, EventArgs.Empty);
            await Task.CompletedTask;
        }

        public Task Report(SerializableTestCase @case)
            => ConsoleReporter.Report(@case);

        internal Action<IList<SerializableTastyCommand>>? CommandsRegistered;
        internal Action? FinishSignaled;
        public void RegisterCommands(IList<SerializableTastyCommand> commands)
        {
            CommandsRegistered?.Invoke(commands);
        }

        public void SignalFinish()
        {
            FinishSignaled?.Invoke();
        }

        public void ClearConsole()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException) { /* Handle is invalid */}
        }

        public static class ConsoleReporter
        {
            public static ColorScheme Scheme = ColorScheme.Default;

            static ConsoleReporter()
                => Console.OutputEncoding = Encoding.UTF8;


            //public static Task ReportSummary(IEnumerable<TestCase> tests)
            //{
            //    var totalTests = tests.Count();
            //    var failedTests = tests.Where(m => m.TestOutcome == TestOutcome.Failed).Count();
            //    var ignoredTests = tests.Where(m => m.TestOutcome == TestOutcome.Ignored).Count();
            //    var inconclusiveTests = tests.Where(m => m.TestOutcome == TestOutcome.NotRun).Count();
            //    var successTests = tests.Where(m => m.TestOutcome == TestOutcome.Success).Count();
            //    var outcome = tests.Where(t => t.TestOutcome > TestOutcome.Ignored).Min(t => t.TestOutcome);

            //    var totalTime = tests.Sum(m => m.Duration);
            //    var failedTime = tests.Where(m => m.TestOutcome == TestOutcome.Failed).Sum(m => m.Duration);
            //    var ignoredTime = tests.Where(m => m.TestOutcome == TestOutcome.Ignored).Sum(m => m.Duration);
            //    var inconclusiveTime = tests.Where(m => m.TestOutcome == TestOutcome.NotRun).Sum(m => m.Duration);
            //    var successTime = tests.Where(m => m.TestOutcome == TestOutcome.Success).Sum(m => m.Duration);

            //    var totalTimeString = totalTime.AsDuration();

            //    Console.WriteLine();
            //    Console.WriteLine(new string('=', SeparatorSize.Value));

            //    Write(Scheme.DefaultColor, $"Summary: ");
            //    Write(failedTests > 0 ? Scheme.ErrorColor : Scheme.DefaultColor, $"F{failedTests}".PadLeft(totalTimeString.Length));
            //    Write(Scheme.DefaultColor, $" | ");
            //    Write(ignoredTests > 0 ? Scheme.WarningColor : Scheme.DefaultColor, $"I{ignoredTests}".PadLeft(totalTimeString.Length));
            //    Write(Scheme.DefaultColor, $" | ");
            //    Write(inconclusiveTests > 0 ? Scheme.NotifyColor : Scheme.DefaultColor, $"NR{inconclusiveTests}".PadLeft(totalTimeString.Length));
            //    Write(Scheme.DefaultColor, $" | ");
            //    Write(successTests > 0 ? Scheme.SuccessColor : Scheme.DefaultColor, $"S{successTests}".PadLeft(totalTimeString.Length));
            //    Write(Scheme.DefaultColor, $" | ");
            //    Write(Scheme.DefaultColor, $"T{totalTests}");

            //    Console.WriteLine();
            //    Write(Scheme.DefaultColor, $"Time:    ");
            //    Write(failedTests > 0 ? Scheme.ErrorColor : Scheme.DefaultColor, failedTime.AsDuration());
            //    Write(Scheme.DefaultColor, $" | ");
            //    Write(ignoredTests > 0 ? Scheme.WarningColor : Scheme.DefaultColor, ignoredTime.AsDuration());
            //    Write(Scheme.DefaultColor, $" | ");
            //    Write(inconclusiveTests > 0 ? Scheme.NotifyColor : Scheme.DefaultColor, inconclusiveTime.AsDuration());
            //    Write(Scheme.DefaultColor, $" | ");
            //    Write(successTests > 0 ? Scheme.SuccessColor : Scheme.DefaultColor, successTime.AsDuration());
            //    Write(Scheme.DefaultColor, $" | ");
            //    Write(Scheme.DefaultColor, totalTimeString);

            //    Console.WriteLine();
            //    Write(Scheme.DefaultColor, $"Outcome: ");
            //    Write(
            //        failedTests > 0
            //            ? Scheme.ErrorColor
            //            : ignoredTests > 0
            //                ? Scheme.WarningColor
            //                : inconclusiveTests > 0
            //                ? Scheme.NotifyColor
            //                : Scheme.SuccessColor
            //                , outcome.ToString().PadLeft(totalTimeString.Length));

            //    Console.WriteLine();
            //    Console.WriteLine(new string('=', SeparatorSize.Value));
            //    Console.WriteLine();

            //    return Task.CompletedTask;
            //}

            public static Task Report(SerializableTestCase test)
                => test.TestOutcome switch
                {
                    TestOutcome.Success => Success(test),
                    TestOutcome.NotRun => NotRun(test),
                    TestOutcome.Ignored => Ignored(test),
                    TestOutcome.Failed => Failed(test),
                    _ => throw new NotImplementedException($"{nameof(ConsoleReporter)}.{nameof(Report)}.{nameof(TestOutcome)}={test.TestOutcome}")
                };

            static string GetTestName(SerializableTestCase test)
                => test.FullName;

            private static Task Success(SerializableTestCase test)
            {
                WriteLine(Scheme.SuccessColor, $"{Scheme.SuccessIcon} {Duration(test)} {GetTestName(test)}");
                if (!string.IsNullOrEmpty(test.AdditionalMessage))
                {
                    WriteLine(Scheme.SuccessColor, $"\t{test.AdditionalMessage}");
                }
                return Task.CompletedTask;
            }

            private static Task NotRun(SerializableTestCase test)
            {
                WriteLine(Scheme.NotifyColor, $"{Scheme.NotRunIcon} {Duration(test)} {GetTestName(test)}");
                return Task.CompletedTask;
            }

            private static Task Ignored(SerializableTestCase test)
            {
                WriteLine(Scheme.WarningColor, $"{Scheme.IgnoredIcon} {Duration(test)} {GetTestName(test)}");
                if (!string.IsNullOrEmpty(test.IgnoredReason))
                {
                    WriteLine(Scheme.WarningColor, $"\t{test.IgnoredReason}");
                }
                return Task.CompletedTask;
            }

            private static Task Failed(SerializableTestCase test)
            {
                WriteLine(Scheme.ErrorColor, $"{Scheme.ErrorIcon} {Duration(test)} {GetTestName(test)}");
                if (test.Exception != null)
                {
                    WriteLine(Scheme.ErrorColor, $"\t{test.Exception}");
                }
                if (!string.IsNullOrEmpty(test.AdditionalMessage))
                {
                    WriteLine(Scheme.ErrorColor, $"\t{test.AdditionalMessage}");
                }
                return Task.CompletedTask;
            }

            private static string Duration(SerializableTestCase test)
                => test.Duration.AsDuration();

            private static void WriteLine(ConsoleColor color, string formattableString)
                => Finally(() =>
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine(formattableString);
                }, ResetColor);

            private static void ResetColor()
                => Console.ResetColor();
        }
    }
}

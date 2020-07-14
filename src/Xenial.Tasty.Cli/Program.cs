using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Pipes;
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

                        var uiTask = Task.Run(async () => await WaitForInput(server));

                        Console.WriteLine("Connected. NamedPipeServerStream...");
                        try
                        {
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
            catch(TaskCanceledException)
            {
                return 1;
            }
        }

        static Task WaitForInput(TastyServer tastyServer)
        {
            var cts = new CancellationTokenSource();
            return Task.Run(() =>
            {
                Console.CancelKeyPress += (_, e) =>
                {
                    if (e.Cancel)
                    {
                        Console.WriteLine("Cancelling execution...");
                        tastyServer.DoRequestExit();
                        cts.Cancel();
                        throw new TaskCanceledException(null, null, cts.Token);
                    }
                };

                Action? ReadInput()
                {
                    Console.WriteLine("Interactive Mode");
                    Console.WriteLine("================");
                    Console.WriteLine("e - Execute Tests");
                    Console.WriteLine("Enter - Execute Tests");
                    Console.WriteLine("c - Cancel");
                    Console.WriteLine("x - Force Exit");
                    Console.WriteLine("================");
                    var info = Console.ReadKey();
                    Console.WriteLine($"Executing {info.KeyChar}");
                    if (info.Key == ConsoleKey.E || info.Key == ConsoleKey.Enter)
                    {
                        return () =>
                        {
                            tastyServer.DoExecuteCommand(new ExecuteCommandEventArgs
                            {
                                CommandName = "Execute"
                            });
                        };
                    }
                    if (info.Key == ConsoleKey.C)
                    {
                        tastyServer.DoRequestCancellation();
                        cts.Cancel();
                        return null;
                    }
                    if (info.Key == ConsoleKey.X)
                    {
                        tastyServer.DoRequestExit();
                        cts.Cancel();
                        return null;
                    }
                    return () => ReadInput();
                }
                var action = ReadInput();
                while (!cts.IsCancellationRequested && action != null)
                {
                    action();
                    action = ReadInput();
                }

            }, cts.Token);
        }
    }

    public interface TastyClient
    {
        Task ExecuteCommand(string command);
    }

    public class TastyServer
    {
        public event EventHandler<ExecuteCommandEventArgs>? ExecuteCommand;
        public event EventHandler? CancellationRequested;
        public event EventHandler? Exit;

        internal void DoExecuteCommand(ExecuteCommandEventArgs args) => this.ExecuteCommand?.Invoke(this, args);
        internal void DoRequestCancellation() => this.CancellationRequested?.Invoke(this, EventArgs.Empty);
        internal void DoRequestExit() => this.Exit?.Invoke(this, EventArgs.Empty);

        public Task Report(SerializableTestCase @case)
        {
            return ConsoleReporter.Report(@case);
        }

        public static class ConsoleReporter
        {
            public static ColorScheme Scheme = ColorScheme.Default;

            static ConsoleReporter()
                => Console.OutputEncoding = Encoding.UTF8;

            static Lazy<int> SeparatorSize = new Lazy<int>(() =>
            {
                const int fallBackSize = 100;
                try
                {
                    return Console.BufferWidth;
                }
                catch (IOException) { /* Handle is invalid */ }
                return fallBackSize;
            });


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

            private static void WriteLine(ConsoleColor color, FormattableString formattableString)
                => Finally(() =>
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine(formattableString);
                }, ResetColor);

            private static void Write(ConsoleColor color, string formattableString)
                => Finally(() =>
                {
                    Console.ForegroundColor = color;
                    Console.Write(formattableString);
                }, ResetColor);

            private static void Write(ConsoleColor color, FormattableString formattableString)
                => Finally(() =>
                {
                    Console.ForegroundColor = color;
                    Console.Write(formattableString);
                }, ResetColor);

            private static void ResetColor()
                => Console.ResetColor();
        }
    }
}

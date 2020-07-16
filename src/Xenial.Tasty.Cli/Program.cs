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

                tastyServer.TestRunEndSignaled = () =>
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
}

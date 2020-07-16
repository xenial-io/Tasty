using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using StreamJsonRpc;

using Xenial.Delicious.Protocols;

using static SimpleExec.Command;
using static Xenial.Delicious.Cli.Helpers.PromiseHelper;

namespace Xenial.Delicious.Cli
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
                            Func<TastyServer, Task<IList<SerializableTastyCommand>>> waitForCommands = (TastyServer tastyServer) =>
                                Promise<IList<SerializableTastyCommand>>((resolve, reject) =>
                                {
                                    var cts = new CancellationTokenSource();
                                    cts.CancelAfter(10000);
                                    cts.Token.Register(() => reject(cts.Token));
                                    tastyServer.CommandsRegistered = (c) =>
                                    {
                                        cts.Dispose();
                                        resolve(c);
                                    };
                                });

                            var uiTask = Task.Run(async () =>
                            {
                                var commands = await waitForCommands(server);
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

        static Task WaitForInput(IList<SerializableTastyCommand> commands, TastyServer tastyServer)
            => Promise((resolve) =>
            {
                Func<Task> cancelKeyPress = () => Promise((resolve, reject) =>
               {
                   Console.CancelKeyPress += (_, e) =>
                   {
                       if (e.Cancel)
                       {
                           Console.WriteLine("Cancelling execution...");
                           reject();
                       }
                   };
               });

                Func<Task> endTestPipelineSignaled = () => Promise((resolve, reject) =>
                {
                    tastyServer.EndTestPipelineSignaled = () =>
                    {
                        Console.WriteLine("Cancelling execution...");
                        reject();
                    };
                });

                Func<Task<ConsoleKeyInfo>> readConsoleKey = () => Promise<ConsoleKeyInfo>(resolve =>
                {
                    _ = Task.Run(() =>
                    {
                        var info = Console.ReadKey(true);
                        resolve(info);
                    });
                });

                Action writeCommands = () =>
                {
                    Console.WriteLine("Interactive Mode");
                    Console.WriteLine("================");

                    foreach (var c in commands)
                    {
                        Console.WriteLine($"{c.Name} - {c.Description}" + (c.IsDefault ? " (default)" : string.Empty));
                    }

                    Console.WriteLine("c - Cancel");
                    Console.WriteLine("================");
                };

                Func<ConsoleKeyInfo, SerializableTastyCommand?> findCommand = (info) =>
                {
                    var key = info.Key.ToString().ToLower();

                    var command = info.Key == ConsoleKey.Enter
                        ? commands.FirstOrDefault(c => c.IsDefault)
                        : commands.FirstOrDefault(c => c.Name.ToLowerInvariant() == key);

                    return command;
                };

                Func<Task<Func<Task>>> readInput = async () =>
                {
                    var info = await readConsoleKey();
                    var command = findCommand(info);
                    if (command != null)
                    {
                        return async () =>
                        {
                            Console.WriteLine($"Executing {command.Name} - {command.Description}");

                            await tastyServer.DoExecuteCommand(new ExecuteCommandEventArgs
                            {
                                CommandName = command.Name
                            });
                        };
                    }
                    if (info.Key == ConsoleKey.C)
                    {
                        return async () =>
                        {
                            Console.WriteLine($"Requesting cancellation");

                            await tastyServer.DoRequestCancellation();
                        };
                    }

                    return () =>
                    {
                        writeCommands();
                        return Task.CompletedTask;
                    };
                };

                return Promise(async (resolve) =>
                {
                    writeCommands();
                    var input = await readInput();
                    var cancel = cancelKeyPress();
                    var endTestPipeline = endTestPipelineSignaled();
                    await Task.WhenAny(input(), cancel, endTestPipeline);
                    resolve();
                });
            });
    }
}

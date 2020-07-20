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

using Terminal.Gui;

using Xenial.Delicious.Protocols;

using static SimpleExec.Command;
using static Xenial.Delicious.Utils.PromiseHelper;

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

            var studioCommand = new Command("studio")
            {
                Handler = CommandHandler.Create<CancellationToken>(Studio)
            };
            studioCommand.AddAlias("s");
            rootCommand.Add(studioCommand);

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
                                        tastyServer.CommandsRegistered = null;
                                        resolve(c);
                                    };
                                });

                            var uiTask = Task.Run(async () =>
                            {
                                var commands = await waitForCommands(server);
                                await Task.Run(async () => await WaitForInput(commands, server), cancellationToken);
                                Console.WriteLine("UI-Task ended");
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
            => Promise(async (resolve) =>
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

                Func<Task> endTestPipelineSignaled = () => Promise((resolve) =>
                {
                    tastyServer.EndTestPipelineSignaled = () =>
                    {
                        Console.WriteLine("Pipeline ended.");
                        resolve();
                    };
                });

                Func<Task> testPipelineCompletedSignaled = () => Promise((resolve) =>
                {
                    tastyServer.TestPipelineCompletedSignaled = () =>
                    {
                        Console.WriteLine("Pipeline completed.");
                        resolve();
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
                        return () => Promise(async (_, reject) =>
                        {
                            Console.WriteLine($"Requesting cancellation");

                            await tastyServer.DoRequestCancellation();
                            reject();
                        });
                    }

                    return () => Task.CompletedTask;
                };

                Task waitForInput() => Promise(async (resolve, reject) =>
                {
                    writeCommands();
                    var input = await readInput();

                    var cancel = cancelKeyPress();
                    var endTestPipeline = endTestPipelineSignaled();
                    var completedTestPipleLine = testPipelineCompletedSignaled();

                    try
                    {
                        var inputTask = input();
                        if (inputTask.IsCanceled)
                        {
                            reject();
                            return;
                        }
                        var result = await Task.WhenAny(cancel, endTestPipeline, completedTestPipleLine);
                    }
                    catch (TaskCanceledException)
                    {
                        reject();
                        return;
                    }
                    if (endTestPipeline.IsCompletedSuccessfully)
                    {
                        resolve();
                        return;
                    }

                    await waitForInput();
                });
                await waitForInput();
                resolve();
            });

        static Task<int> Studio(CancellationToken cancellationToken)
        {
            Application.Init();
            var top = Application.Top;

            // Creates the top-level window to show
            var win = new Window("MyApp")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_New", "Creates new file", () => {}),
                new MenuItem ("_Close", "", () => { }),
                new MenuItem ("_Quit", "", () => { top.Running = false; })
            }),
            new MenuBarItem ("_Edit", new MenuItem [] {
                new MenuItem ("_Copy", "", null),
                new MenuItem ("C_ut", "", null),
                new MenuItem ("_Paste", "", null)
            })
        });
            top.Add(menu);

            var login = new Label("Login: ") { X = 3, Y = 2 };
            var password = new Label("Password: ")
            {
                X = Pos.Left(login),
                Y = Pos.Top(login) + 1
            };
            var loginText = new TextField("")
            {
                X = Pos.Right(password),
                Y = Pos.Top(login),
                Width = 40
            };
            var passText = new TextField("")
            {
                Secret = true,
                X = Pos.Left(loginText),
                Y = Pos.Top(password),
                Width = Dim.Width(loginText)
            };

            // Add some controls, 
            win.Add(
                // The ones with my favorite layout system
                login, password, loginText, passText,

                    // The ones laid out like an australopithecus, with absolute positions:
                    new CheckBox(3, 6, "Remember me"),
                    new RadioGroup(3, 8, new[] { "_Personal", "_Company" }),
                    new Button(3, 14, "Ok"),
                    new Button(10, 14, "Cancel"),
                    new Label(3, 18, "Press F9 or ESC plus 9 to activate the menubar"));

            Application.Run();
            return Task.FromResult(1);
        }
    }
}

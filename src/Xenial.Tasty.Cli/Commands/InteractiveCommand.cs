using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Plugins;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Transports;

using static Xenial.Delicious.Utils.PromiseHelper;

namespace Xenial.Delicious.Cli.Commands
{
    public static class InteractiveCommand
    {
        public static async Task<int> Interactive(string project, CancellationToken cancellationToken)
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
                        var connectionString = NamedPipesConnectionStringBuilder.CreateNewConnection();

                        Console.WriteLine(csProjFileName);
                        var commander = new TastyProcessCommander(connectionString);

                        commander.UseNamedPipesTransport()
                                 .RegisterReporter(ConsoleReporter.Report)
                                 .RegisterReporter(ConsoleReporter.ReportSummary);

                        await commander.BuildProject(path, new Progress<(string line, bool isRunning, int exitCode)>(p =>
                        {
                            Console.WriteLine(p.line);
                        }), cancellationToken).ConfigureAwait(false);

                        Console.WriteLine("Connecting to remote");
                        var remoteTask = await commander.ConnectToRemote(path, cancellationToken: cancellationToken).ConfigureAwait(false);
                        Console.WriteLine("Connected to remote");

                        try
                        {
                            var uiTask = Task.Run(async () =>
                            {
                                var commands = await commander.ListCommands(cancellationToken).ConfigureAwait(false);
                                await Task.Run(async () => await WaitForInput(commands, commander).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
                                Console.WriteLine("UI-Task ended");
                            }, cancellationToken);

                            await Task.WhenAll(remoteTask, uiTask).ConfigureAwait(false);
                        }
                        catch (TaskCanceledException)
                        {
                            return 0;
                        }
                        catch (SimpleExec.NonZeroExitCodeException e)
                        {
                            return e.ExitCode;
                        }
                        finally
                        {
                            commander.Dispose();
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

        private static Task WaitForInput(IList<SerializableTastyCommand> commands, TastyCommander commander)
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
                    commander.EndTestPipelineSignaled = () =>
                    {
                        Console.WriteLine("Pipeline ended.");
                        resolve();
                    };
                });

                Func<Task> testPipelineCompletedSignaled = () => Promise((resolve) =>
                {
                    commander.TestPipelineCompletedSignaled = () =>
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
                    var command = info.Key == ConsoleKey.Enter
                        ? commands.FirstOrDefault(c => c.IsDefault)
                        : commands.FirstOrDefault(c => string.Equals(c.Name, info.Key.ToString(), StringComparison.OrdinalIgnoreCase));

                    return command;
                };

                Func<Task<Func<Task>>> readInput = async () =>
                {
                    var info = await readConsoleKey().ConfigureAwait(false);
                    var command = findCommand(info);
                    if (command != null)
                    {
                        return async () =>
                        {
                            Console.WriteLine($"Executing {command.Name} - {command.Description}");

                            await commander.DoExecuteCommand(new ExecuteCommandEventArgs
                            {
                                CommandName = command.Name
                            }).ConfigureAwait(false);
                        };
                    }
                    if (info.Key == ConsoleKey.C)
                    {
                        return () => Promise(async (_, reject) =>
                        {
                            Console.WriteLine($"Requesting cancellation");

                            await commander.DoRequestCancellation().ConfigureAwait(false);
                            reject();
                        });
                    }

                    return () => Task.CompletedTask;
                };

                Task waitForInput() => Promise(async (resolve, reject) =>
                {
                    writeCommands();
                    var input = await readInput().ConfigureAwait(false);

                    var cancel = cancelKeyPress();
                    var endTestPipeline = endTestPipelineSignaled();
                    var completedTestPipeLine = testPipelineCompletedSignaled();

                    try
                    {
                        var inputTask = input();
                        if (inputTask.IsCanceled)
                        {
                            reject();
                            return;
                        }
                        var result = await Task.WhenAny(cancel, endTestPipeline, completedTestPipeLine).ConfigureAwait(false);
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

                    await waitForInput().ConfigureAwait(false);
                });
                await waitForInput().ConfigureAwait(false);
                resolve();
            });

    }
}

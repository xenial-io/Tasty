using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StreamJsonRpc;
using Terminal.Gui;
using Xenial.Delicious.Cli.Internal;
using Xenial.Delicious.Cli.Views;
using Xenial.Delicious.Protocols;
using static SimpleExec.Command;
using static Xenial.Delicious.Utils.PromiseHelper;

namespace Xenial.Delicious.Cli.Commands
{
    public static class StudioCommand
    {
        private static MenuBar? _menu;
        private static FrameView? _leftPane;
        private static FrameView? _rightPane;
        private static ListView? _commandsListView;
        private static Toplevel? _top;
        private static StatusBar? _statusBar;
        public static Task<int> Studio()
        {
            Application.Init();
            _top = Application.Top;
            using var cancellationTokenSource = new CancellationTokenSource();

            _leftPane = new FrameView("Commands")
            {
                X = 0,
                Y = 1, // for menu
                Width = 25,
                Height = Dim.Fill(1),
                CanFocus = false,
            };

            _rightPane = new FrameView("Output")
            {
                X = 25,
                Y = 1, // for menu
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                CanFocus = true,
            };

            SetColorScheme();

            _menu = new MenuBar(new[]
            {
                new MenuBarItem ("_File", new[]
                {
                    new MenuItem ("_Open", "", async () =>
                    {
                        await ShowOpenProjectAndBuildDialog(cancellationTokenSource);
                    }),
                    new MenuItem ("_Quit", "", () =>
                    {
                        Application.RequestStop();
                    })
                }),
                new MenuBarItem("_Color Scheme", CreateColorSchemeMenuItems()),
                new MenuBarItem("_About...", "About Tasty.Cli", () => MessageBox.Query("About Tasty.Cli", aboutMessage.ToString(), "_Ok")),
            });

            _statusBar = new StatusBar(new[]
            {
                new StatusItem(Key.ControlQ, "~CTRL-Q~ Quit", () =>
                {
                    Application.RequestStop();
                }),
                new StatusItem(Key.ControlD, "~CTRL-D~ Debug", () =>
                {
                    if (!Debugger.IsAttached)
                    {
                        Debugger.Launch();
                    }
                }),
                new StatusItem(Key.ControlC, "~CTRL-C~ Cancel", () =>
                {
                    cancellationTokenSource.Cancel();
                }),
                new StatusItem(Key.F9, "~F9~ Open Menu", () =>
                {
                    _menu.OpenMenu();
                }),
            });

            _top.Add(_menu, _leftPane, _rightPane, _statusBar);

            _top.Initialized += async (_, __) =>
            {
                await ShowOpenProjectAndBuildDialog(cancellationTokenSource);
            };

            Application.Run();
            return Task.FromResult(1);
        }

        private static async Task ShowOpenProjectAndBuildDialog(CancellationTokenSource cancellationTokenSource)
        {
            var path = await SelectProjectDialog.ShowDialogAsync(_baseColorScheme!);
            if (path != null)
            {
                var commander = new TastyCommander();

                var logView = new TextView
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                    ReadOnly = true
                };
                _rightPane?.Clear();
                _rightPane?.Add(logView);
                var log = string.Empty;
                var progress = new Progress<(string line, bool isRunning, int exitCode)>(p =>
                {
                    log += $"{p.line.Trim()}{Environment.NewLine}";
                    logView.Text = log;
                });

                var sw = Stopwatch.StartNew();
                var exitCode = await commander.BuildProject(path, progress, cancellationTokenSource.Token);
                ((IProgress<(string line, bool isRunning, int exitCode)>)progress).Report(($"Finished in {sw.Elapsed}", true, exitCode));
                await CreateCommandsListView(path, logView, cancellationTokenSource);
            }
        }

        static async Task CreateCommandsListView(string csProjFileName, TextView logView, CancellationTokenSource cancellationTokenSource)
        {
            if (_commandsListView == null)
            {
                var connectionId = $"TASTY_{Guid.NewGuid()}";

                using var stream = new NamedPipeServerStream(connectionId, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                var connectionTask = stream.WaitForConnectionAsync(cancellationTokenSource.Token);
                var remoteTask = ReadAsync("dotnet",
                    $"run -p \"{csProjFileName}\" -f netcoreapp3.1 --no-restore --no-build",
                    noEcho: true,
                    configureEnvironment: (env) =>
                    {
                        env.Add("TASTY_INTERACTIVE", "true");
                        env.Add("TASTY_INTERACTIVE_CON_TYPE", "NamedPipes");
                        env.Add("TASTY_INTERACTIVE_CON_ID", connectionId);
                    }
                );

                logView.Text += "Connecting. NamedPipeServerStream...";
                await connectionTask;
                var server = new TastyServer();
                using var tastyServer = JsonRpc.Attach(stream, server);

                logView.Text += "Connected. NamedPipeServerStream...";

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

                _commandsListView = new ListView(Array.Empty<object>())
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(0),
                    Height = Dim.Fill(0),
                    AllowsMarking = false,
                    CanFocus = true,
                };

                _commandsListView.OpenSelectedItem += (a) =>
                {
                    _top?.SetFocus(_rightPane);
                };

                _leftPane?.Add(_commandsListView);

                try
                {
                    var commands = await waitForCommands(server);
                    if (commands != null)
                    {
                        await _commandsListView.SetSourceAsync(commands.Select(c => c.Description ?? c.Name).ToList());
                    }
                    _top?.SetFocus(_commandsListView);
                }
                catch (Exception ex)
                {
                    logView.Text += ex.ToString();
                    _top?.SetFocus(logView);
                }
            }
            else
            {
                _top?.SetFocus(_commandsListView);
            }
        }

        static void SetColorScheme()
        {
            if (_baseColorScheme == null)
            {
                _baseColorScheme = Colors.ColorSchemes["Base"];
            }
#pragma warning disable CS8601 // Possible null reference assignment.
            _leftPane!.ColorScheme = _baseColorScheme;
            _rightPane!.ColorScheme = _baseColorScheme;
            _top!.ColorScheme = _baseColorScheme;
#pragma warning restore CS8601 // Possible null reference assignment.
            _top?.SetNeedsDisplay();
        }

        static ColorScheme? _baseColorScheme;
        static MenuItem[] CreateColorSchemeMenuItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>(Colors.ColorSchemes.Count);
            foreach (var sc in Colors.ColorSchemes)
            {
                var item = new MenuItem();
                item.Title = sc.Key;
                item.CheckType |= MenuItemCheckStyle.Radio;

#pragma warning disable CS8604 // Possible null reference argument.
                item.Checked = sc.Value == _baseColorScheme;
#pragma warning restore CS8604 // Possible null reference argument.

                item.Action += () =>
                {
                    _baseColorScheme = sc.Value;
                    SetColorScheme();
                    foreach (var menuItem in menuItems)
                    {
                        menuItem.Checked = menuItem.Title.Equals(sc.Key) && sc.Value == _baseColorScheme;
                    }
                };
                menuItems.Add(item);
            }
            return menuItems.ToArray();
        }

        private const string aboutMessage = @"
     Tasty
    delicious dotnet testing

                 .@@@@@@@@              
               @@@@%********%@@/        
             %@,     &@(/&@/,,,,(@@     
           @&******/%@@#.  %@%*,,,*&*   
        %@/.,*#&(,#*,,,,*(@@(//@(,,,(@, 
       .@(..........,%@/,,,,#@@%&@%*,*@#
       @(......(@#......(@/,,,%#  ,@/,#@
      @%......**.*........(@*,,#@.*@*,*@
     @%..........*@@&.......&#,,(%  &(,@
    @@............/#..*@@....%(,*@* (@@#
   %&//..............,*......*@/*&&@@   
  @@/////,....................@#%@#     
 (@(///////*................,%@/        
 @*..*////////,......,%@@@/             
@(......///////&@@@#                    
@@*....,%@@@&                           
";
    }
}

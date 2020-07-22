using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

using Xenial.Delicious.Cli.Internal;

namespace Xenial.Delicious.Cli.Commands
{
    public static class StudioCommand
    {
        private static MenuBar? _menu;
        private static FrameView? _leftPane;
        private static FrameView? _rightPane;
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

            // Creates a menubar, the item "New" has a help menu.
            _menu = new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem ("_File", new []
                {
                    new MenuItem ("_Open", "", async () =>
                    {
                        var dialog = new OpenDialog
                        {
                            AllowedFileTypes = new [] { "csproj", "exe", "dll"},
                            CanChooseDirectories = false,
                            AllowsMultipleSelection = false
                        };
                        Application.Run(dialog);
                        var filePath = dialog.FilePath;
                        if(filePath != null)
                        {
                            var path = filePath.ToString();
                            if(!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                            {
                                var commander = new TastyCommander();

                                var logView = new TextView () {
                                    X = 0,
                                    Y = 0,
                                    Width = Dim.Fill(),
                                    Height = Dim.Fill(),
                                    ReadOnly = true
                                };

                                _rightPane.Add(logView);
                                var log = string.Empty;
                                var progress = new Progress<(string line, bool isRunning, int exitCode)>(p =>
                                {
                                    log += $"{p.line.Trim()}{Environment.NewLine}";
                                    logView.Text = log;
                                });

                                var sw = Stopwatch.StartNew();
                                var exitCode = await commander.BuildProject(path, progress, cancellationTokenSource.Token);
                                ((IProgress<(string line, bool isRunning, int exitCode)>)progress).Report(($"Finished in {sw.Elapsed}", true, exitCode));
                            }
                        }
                    }
                    ),
                    new MenuItem ("_Quit", "", () => Application.RequestStop() )
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

            SetColorScheme();

            Application.Run();
            return Task.FromResult(1);
        }

        static void SetColorScheme()
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            _leftPane!.ColorScheme = _baseColorScheme;
            _rightPane!.ColorScheme = _baseColorScheme;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Terminal.Gui;

namespace Xenial.Delicious.Cli.Views
{
    internal class MainWindow : IDisposable
    {
        internal Toplevel Top { get; }

        FrameView _LeftPane;
        FrameView _RightPane;
        TextView _LogView;
        MenuBar _Menu;
        StatusBar _StatusBar;
        MenuItem[] _ColorSchemeMenuItems;
        MainWindowViewModel _ViewModel;

        public MainWindow(MainWindowViewModel viewModel, Toplevel top)
        {
            _ViewModel = viewModel;
            Top = top;
            _LeftPane = new FrameView("Commands")
            {
                X = 0,
                Y = 1, // for menu
                Width = 25,
                Height = Dim.Fill(1),
                CanFocus = false,
            };

            _RightPane = new FrameView("Output")
            {
                X = 25,
                Y = 1, // for menu
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                CanFocus = false,
            };

            _LogView = new TextView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true
            };

            _RightPane.Add(_LogView);

            _ColorSchemeMenuItems = CreateColorSchemeMenuItems();

            _Menu = new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem ("_File", new[]
                {
                    new MenuItem ("_Open", "", async () =>
                    {
                        await viewModel.ShowOpenProjectDialog();
                    }),
                    new MenuItem ("_Quit", "", async () =>
                    {
                        await viewModel.StopApplication();
                    })
                }),
                new MenuBarItem("_Color Scheme", _ColorSchemeMenuItems),
                new MenuBarItem("_About...", "About Tasty.Cli", async () => await AboutTastyDialog.ShowDialogAsync()),
            });

            _StatusBar = new StatusBar(new[]
            {
                new StatusItem(Key.ControlQ, "~CTRL-Q~ Quit", async () =>
                {
                    await viewModel.StopApplication();
                }),
                new StatusItem(Key.ControlD, "~CTRL-D~ Debug", async () =>
                {
                    await viewModel.LaunchDebugger();
                }),
                new StatusItem(Key.ControlC, "~CTRL-C~ Cancel", async () =>
                {
                    await viewModel.Cancel();
                }),
                new StatusItem(Key.F6, "~F6~ Clear Log", async () =>
                {
                    await _ViewModel.ClearLog();
                }),
                new StatusItem(Key.F9, "~F9~ Open Menu", () =>
                {
                    _Menu.OpenMenu();
                }),
            });

            Top.Add(_Menu, _LeftPane, _RightPane, _StatusBar);

            SetColor(_ViewModel.ColorSchemeName, _ViewModel.ColorScheme);

            viewModel.LogProgress.ProgressChanged += LogProgress_ProgressChanged;
            Top.Initialized += Top_Initialized;
        }

        public MainWindow(MainWindowViewModel viewModel) : this(viewModel, Application.Top) { }

        MenuItem[] CreateColorSchemeMenuItems()
             => Colors.ColorSchemes.Select(color => new
             {
                 Name = color.Key,
                 Color = color.Value,
                 MenuItem = new MenuItem
                 {
                     Title = color.Key,
                     CheckType = MenuItemCheckStyle.Radio,
                     Action = () => SetColor(color.Key, color.Value)
                 }
             })
            .Select(m => m.MenuItem)
            .ToArray();

        void SetColor(string colorSchemeName, ColorScheme colorScheme)
        {
            _ViewModel.SetColor(colorSchemeName, colorScheme);

            foreach (var menuItem in _ColorSchemeMenuItems)
            {
                menuItem.Checked = false;
            }

            var menuItemToCheck = _ColorSchemeMenuItems.FirstOrDefault(m => m.Title.Equals(colorSchemeName));
            if (menuItemToCheck != null)
            {
                menuItemToCheck.Checked = true;
            }

            _LeftPane.ColorScheme = colorScheme;
            _RightPane.ColorScheme = colorScheme;
            Top.ColorScheme = colorScheme;
            Top.SetNeedsDisplay();
        }

        private async void Top_Initialized(object? sender, EventArgs e)
            => await _ViewModel.ShowOpenProjectDialog();

        private void LogProgress_ProgressChanged(object? sender, (string line, bool isRunning, int exitCode) e)
            => _LogView.Text = _ViewModel.LogText;

        public void Dispose()
            => _ViewModel.Dispose();
    }
}

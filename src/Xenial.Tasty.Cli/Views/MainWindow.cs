using System;
using System.Linq;

using Terminal.Gui;

namespace Xenial.Delicious.Cli.Views
{
    internal class MainWindow : IDisposable
    {
        internal Toplevel Top { get; }

        private readonly FrameView leftPane;
        private readonly ListView commandsListView;
        private readonly FrameView rightPane;
        private readonly TextView logView;
        private readonly MenuBar menu;
        private readonly StatusBar statusBar;
        private readonly MenuItem[] colorSchemeMenuItems;
        private readonly MainWindowViewModel viewModel;

        public MainWindow(MainWindowViewModel viewModel, Toplevel top)
        {
            this.viewModel = viewModel;
            Top = top;
            leftPane = new FrameView("Commands")
            {
                X = 0,
                Y = 1, // for menu
                Width = 25,
                Height = Dim.Fill(1),
                CanFocus = false,
            };

            commandsListView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = false,
            };

            leftPane.Add(commandsListView);

            rightPane = new FrameView("Output")
            {
                X = 25,
                Y = 1, // for menu
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                CanFocus = false,
            };

            logView = new TextView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true,
                CanFocus = true
            };

            rightPane.Add(logView);

            colorSchemeMenuItems = CreateColorSchemeMenuItems();

            menu = new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem ("_File", new[]
                {
                    new MenuItem ("_Open", "", async () =>
                    {
                        await viewModel.ShowOpenProjectDialog().ConfigureAwait(true);
                    }),
                    new MenuItem ("_Quit", "", async () =>
                    {
                        await viewModel.StopApplication().ConfigureAwait(true);
                    })
                }),
                new MenuBarItem("_Color Scheme", colorSchemeMenuItems),
                new MenuBarItem("_About...", "About Tasty.Cli", async () => await AboutTastyDialog.ShowDialogAsync().ConfigureAwait(true)),
            });

            statusBar = new StatusBar(new[]
            {
                new StatusItem(Key.ControlQ, "~CTRL-Q~ Quit", async () =>
                {
                    await viewModel.StopApplication().ConfigureAwait(true);
                }),
                new StatusItem(Key.ControlD, "~CTRL-D~ Debug", async () =>
                {
                    await viewModel.LaunchDebugger().ConfigureAwait(true);
                }),
                new StatusItem(Key.ControlC, "~CTRL-C~ Cancel", async () =>
                {
                    await viewModel.Cancel().ConfigureAwait(true);
                }),
                new StatusItem(Key.F6, "~F6~ Clear Log", async () =>
                {
                    await this.viewModel.ClearLog().ConfigureAwait(true);
                }),
                new StatusItem(Key.F9, "~F9~ Open Menu", () =>
                {
                    menu.OpenMenu();
                }),
            });

            Top.Add(menu, leftPane, rightPane, statusBar);

            SetColor(this.viewModel.ColorSchemeName, this.viewModel.ColorScheme);

            commandsListView.OpenSelectedItem += CommandsListView_OpenSelectedItem;
            this.viewModel.LogProgress.ProgressChanged += LogProgress_ProgressChanged;
            this.viewModel.Commands.CollectionChanged += Commands_CollectionChanged;
            Top.Initialized += Top_Initialized;
            logView.SetFocus();
        }

        private async void Commands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await commandsListView.SetSourceAsync(viewModel.Commands.ToList()).ConfigureAwait(true);
            commandsListView.CanFocus = viewModel.Commands.Count > 0;
            if (commandsListView.CanFocus)
            {
                commandsListView.SetFocus();
            }
        }

        public MainWindow(MainWindowViewModel viewModel) : this(viewModel, Application.Top) { }

        private MenuItem[] CreateColorSchemeMenuItems()
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

        private void SetColor(string colorSchemeName, ColorScheme colorScheme)
        {
            viewModel.SetColor(colorSchemeName, colorScheme);

            foreach (var menuItem in colorSchemeMenuItems)
            {
                menuItem.Checked = false;
            }

            var menuItemToCheck = colorSchemeMenuItems.FirstOrDefault(m => m.Title.Equals(colorSchemeName));
            if (menuItemToCheck != null)
            {
                menuItemToCheck.Checked = true;
            }

            leftPane.ColorScheme = colorScheme;
            rightPane.ColorScheme = colorScheme;
            Top.ColorScheme = colorScheme;
            Top.SetNeedsDisplay();
        }

        private async void Top_Initialized(object? sender, EventArgs e)
            => await viewModel.ShowOpenProjectDialog().ConfigureAwait(false);

        private void LogProgress_ProgressChanged(object? sender, (string line, bool isError, int? exitCode) e)
            => logView.Text = viewModel.LogText;

        private async void CommandsListView_OpenSelectedItem(ListViewItemEventArgs e)
        {
            var commandItem = (CommandItem)e.Value;

            rightPane.SetFocus();

            await viewModel.ExecuteCommand(commandItem).ConfigureAwait(false);
        }

        public void Dispose()
        {
            commandsListView.OpenSelectedItem -= CommandsListView_OpenSelectedItem;
            viewModel.LogProgress.ProgressChanged -= LogProgress_ProgressChanged;
            viewModel.Commands.CollectionChanged -= Commands_CollectionChanged;
            Top.Initialized -= Top_Initialized;

            viewModel.Dispose();
        }
    }
}

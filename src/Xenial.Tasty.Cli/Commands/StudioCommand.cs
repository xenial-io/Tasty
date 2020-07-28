using System.Threading.Tasks;

using Terminal.Gui;

using Xenial.Delicious.Cli.Views;

namespace Xenial.Delicious.Cli.Commands
{
    public static class StudioCommand
    {
        public static Task<int> Studio()
        {
            Application.Init();

            using var viewModel = new MainWindowViewModel("Base", Colors.ColorSchemes["Base"]);
            using var mainWindow = new MainWindow(viewModel, Application.Top);

            Application.Run(mainWindow.Top);

            return Task.FromResult(1);
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

using Xenial.Delicious.Cli.Internal;

namespace Xenial.Delicious.Cli.Views
{
    internal class MainWindowViewModel : IDisposable
    {
        internal ColorScheme ColorScheme { get; private set; } = null!; //Trick the compiler for SetColor method
        internal string ColorSchemeName { get; private set; } = null!; //Trick the compiler for SetColor method
        internal TastyCommander Commander { get; } = new TastyCommander();
        internal string LogText { get; private set; } = string.Empty;
        internal Progress<(string line, bool isRunning, int exitCode)> LogProgress { get; }

        public MainWindowViewModel(string colorSchemeName, ColorScheme colorScheme)
        {
            LogProgress = new Progress<(string line, bool isRunning, int exitCode)>(p =>
           {
               LogText += $"{p.line.TrimEnd(Environment.NewLine.ToArray())}{Environment.NewLine}";
           });

            SetColor(colorSchemeName, colorScheme);
        }

        internal void SetColor(string colorSchemeName, ColorScheme colorScheme)
            => (ColorSchemeName, ColorScheme) = (colorSchemeName, colorScheme);

        CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        internal async Task ShowOpenProjectDialog()
        {
            var path = await SelectProjectDialog.ShowDialogAsync(ColorScheme);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            await Commander.BuildProject(path, LogProgress);
        }

        internal Task Cancel()
        {
            CancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        internal Task LaunchDebugger()
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            return Task.CompletedTask;
        }

        internal Task StopApplication()
        {
            Application.RequestStop();
            return Task.CompletedTask;
        }

        internal Task ClearLog()
        {
            LogText = string.Empty;

            //TODO: once we have the last exitCode and isRunning props we need to report those
            ((IProgress<(string line, bool isRunning, int exitCode)>)LogProgress).Report((string.Empty, false, 0));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Commander.Dispose();
        }
    }
}

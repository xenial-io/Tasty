using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Plugins;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Remote;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Transports;
using Xenial.Delicious.Utils;

namespace Xenial.Delicious.Cli.Views
{
    internal class MainWindowViewModel : IDisposable
    {
        private readonly Xenial.Delicious.Reporters.ColorScheme scheme = Xenial.Delicious.Reporters.ColorScheme.Default;
        public int SeparatorSize { get; set; } = 100;
        internal Terminal.Gui.ColorScheme ColorScheme { get; private set; } = null!; //Trick the compiler for SetColor method
        internal string ColorSchemeName { get; private set; } = null!; //Trick the compiler for SetColor method
        internal TastyProcessCommander Commander { get; }
        internal string LogText { get; private set; } = string.Empty;
        internal string CurrentProject { get; private set; } = string.Empty;

        internal Progress<(string line, bool isError, int? exitCode)> LogProgress { get; }
        internal ObservableCollection<CommandItem> Commands { get; } = new ObservableCollection<CommandItem>();
        private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        public MainWindowViewModel(string colorSchemeName, Terminal.Gui.ColorScheme colorScheme)
        {
            var connectionString = NamedPipesConnectionStringBuilder.CreateNewConnection();

            LogProgress = new Progress<(string line, bool isError, int? exitCode)>(p =>
            {
                LogText += $"{p.line.TrimEnd(Environment.NewLine.ToArray())}{Environment.NewLine}";
            });

            Commander = new TastyProcessCommander(connectionString, new Func<ProcessStartInfo>(() => ProcessStartInfoHelper.Create("dotnet", $"run --no-build --no-restore -f netcoreapp3.1 {CurrentProject}", configureEnvironment: env =>
            {
                env[EnvironmentVariables.InteractiveMode] = "true";
            })), LogProgress);

            Commander.UseNamedPipesTransport()
                     .RegisterReporter(Report)
                     .RegisterReporter(ReportSummary);

            SetColor(colorSchemeName, colorScheme);
        }

        internal void SetColor(string colorSchemeName, Terminal.Gui.ColorScheme colorScheme)
            => (ColorSchemeName, ColorScheme) = (colorSchemeName, colorScheme);

        internal async Task ShowOpenProjectDialog()
        {
            var path = await SelectProjectDialog.ShowDialogAsync(ColorScheme).ConfigureAwait(true);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            CurrentProject = path;

            await Commander.ConnectAsync(CancellationTokenSource.Token).ConfigureAwait(true);

            //TODO: ErrorDialog
            var commands = await Commander.ListCommands(CancellationTokenSource.Token).ConfigureAwait(true);
            Commands.Clear();
            foreach (var command in commands)
            {
                Commands.Add(new CommandItem(command));
            }
        }

        internal Task Cancel()
        {
            CancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        internal Task LaunchDebugger()
        {
            _ = this;
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            return Task.CompletedTask;
        }

        internal Task StopApplication()
        {
            _ = this;
            Application.RequestStop();
            return Task.CompletedTask;
        }

        internal Task ClearLog()
        {
            LogText = string.Empty;

            WriteLine();

            return Task.CompletedTask;
        }

        public async Task ExecuteCommand(CommandItem commandItem)
            => await Commander.DoExecuteCommand(new ExecuteCommandEventArgs
            {
                CommandName = commandItem.Command.Name
            }).ConfigureAwait(false);

        //TODO: once we have the last exitCode and isRunning props we need to report those
        private void WriteLine(string line = "")
            => ((IProgress<(string line, bool isRunning, int exitCode)>)LogProgress).Report((line, true, 0));

        public Task ReportSummary(IEnumerable<TestCaseResult> tests)
        {
            var totalTests = tests.Count();
            var failedTests = tests.Where(m => m.TestOutcome == TestOutcome.Failed).Count();
            var ignoredTests = tests.Where(m => m.TestOutcome == TestOutcome.Ignored).Count();
            var notRunTests = tests.Where(m => m.TestOutcome == TestOutcome.NotRun).Count();
            var successTests = tests.Where(m => m.TestOutcome == TestOutcome.Success).Count();
            var outcome = tests.Where(t => t.TestOutcome > TestOutcome.Ignored).Min(t => t.TestOutcome);

            var totalTime = tests.Sum(m => m.Duration);
            var failedTime = tests.Where(m => m.TestOutcome == TestOutcome.Failed).Sum(m => m.Duration);
            var ignoredTime = tests.Where(m => m.TestOutcome == TestOutcome.Ignored).Sum(m => m.Duration);
            var notRunTime = tests.Where(m => m.TestOutcome == TestOutcome.NotRun).Sum(m => m.Duration);
            var successTime = tests.Where(m => m.TestOutcome == TestOutcome.Success).Sum(m => m.Duration);

            var totalTimeString = totalTime.AsDuration();

            WriteLine(new string('=', SeparatorSize));

            var sb = new StringBuilder();
            sb.Append($"Summary: ");
            sb.Append($"F{failedTests}".PadLeft(totalTimeString.Length));
            sb.Append($" | ");
            sb.Append($"I{ignoredTests}".PadLeft(totalTimeString.Length));
            sb.Append($" | ");
            sb.Append($"NR{notRunTests}".PadLeft(totalTimeString.Length));
            sb.Append($" | ");
            sb.Append($"S{successTests}".PadLeft(totalTimeString.Length));
            sb.Append($" | ");
            sb.Append($"T{totalTests}");

            WriteLine(sb.ToString());
            sb.Clear();

            sb.Append($"Time:    ");
            sb.Append(failedTime.AsDuration());
            sb.Append($" | ");
            sb.Append(ignoredTime.AsDuration());
            sb.Append($" | ");
            sb.Append(notRunTime.AsDuration());
            sb.Append($" | ");
            sb.Append(successTime.AsDuration());
            sb.Append($" | ");
            sb.Append(totalTimeString);

            WriteLine(sb.ToString());
            sb.Clear();

            sb.Append($"Outcome: ");
            sb.Append(outcome.ToString().PadLeft(totalTimeString.Length));

            WriteLine(sb.ToString());
            WriteLine(new string('=', SeparatorSize));

            return Task.CompletedTask;
        }

        public Task Report(TestCaseResult test)
            => test.TestOutcome switch
            {
                TestOutcome.Success => Success(test),
                TestOutcome.NotRun => NotRun(test),
                TestOutcome.Ignored => Ignored(test),
                TestOutcome.Failed => Failed(test),
                _ => throw new NotImplementedException($"{nameof(MainWindowViewModel)}.{nameof(Report)}.{nameof(TestOutcome)}={test.TestOutcome}")
            };

        private static string GetTestName(TestCaseResult test)
            => test.FullName;

        private Task Success(TestCaseResult test)
        {
            WriteLine($"{scheme.SuccessIcon} {Duration(test)} {GetTestName(test)}");
            if (!string.IsNullOrEmpty(test.AdditionalMessage))
            {
                WriteLine($"\t{test.AdditionalMessage}");
            }
            return Task.CompletedTask;
        }

        private Task NotRun(TestCaseResult test)
        {
            WriteLine($"{scheme.NotRunIcon} {Duration(test)} {GetTestName(test)}");
            return Task.CompletedTask;
        }

        private Task Ignored(TestCaseResult test)
        {
            WriteLine($"{scheme.IgnoredIcon} {Duration(test)} {GetTestName(test)}");
            if (!string.IsNullOrEmpty(test.IgnoredReason))
            {
                WriteLine($"\t{test.IgnoredReason}");
            }
            return Task.CompletedTask;
        }

        private Task Failed(TestCaseResult test)
        {
            WriteLine($"{scheme.ErrorIcon} {Duration(test)} {GetTestName(test)}");
            if (test.Exception != null)
            {
                WriteLine($"\t{test.Exception}");
            }
            if (!string.IsNullOrEmpty(test.AdditionalMessage))
            {
                WriteLine($"\t{test.AdditionalMessage}");
            }
            return Task.CompletedTask;
        }

        private static string Duration(TestCaseResult test)
            => test.Duration.AsDuration();

        public void Dispose()
            => Commander.Dispose();
    }
}

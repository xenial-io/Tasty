using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

using Xenial.Delicious.Cli.Internal;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Utils;

namespace Xenial.Delicious.Cli.Views
{
    internal class MainWindowViewModel : IDisposable
    {
        private Xenial.Delicious.Reporters.ColorScheme Scheme = Xenial.Delicious.Reporters.ColorScheme.Default;
        public int SeparatorSize { get; set; } = 100;
        internal ColorScheme ColorScheme { get; private set; } = null!; //Trick the compiler for SetColor method
        internal string ColorSchemeName { get; private set; } = null!; //Trick the compiler for SetColor method
        internal TastyCommander Commander { get; }
        internal string LogText { get; private set; } = string.Empty;
        internal Progress<(string line, bool isRunning, int exitCode)> LogProgress { get; }
        internal ObservableCollection<CommandItem> Commands { get; } = new ObservableCollection<CommandItem>();
        CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        public MainWindowViewModel(string colorSchemeName, ColorScheme colorScheme)
        {
            Commander = new TastyCommander()
                .RegisterReporter(Report)
                .RegisterReporter(ReportSummary);

            LogProgress = new Progress<(string line, bool isRunning, int exitCode)>(p =>
            {
                LogText += $"{p.line.TrimEnd(Environment.NewLine.ToArray())}{Environment.NewLine}";
            });

            SetColor(colorSchemeName, colorScheme);
        }

        internal void SetColor(string colorSchemeName, ColorScheme colorScheme)
            => (ColorSchemeName, ColorScheme) = (colorSchemeName, colorScheme);

        internal async Task ShowOpenProjectDialog()
        {
            var path = await SelectProjectDialog.ShowDialogAsync(ColorScheme).ConfigureAwait(true);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            await Commander.BuildProject(path, LogProgress).ConfigureAwait(true);
            await Commander.ConnectToRemote(path, CancellationTokenSource.Token).ConfigureAwait(true);

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
        {
            await Commander.DoExecuteCommand(new ExecuteCommandEventArgs
            {
                CommandName = commandItem.Command.Name
            }).ConfigureAwait(false);
        }
        private void WriteLine(string line = "")
        {
            //TODO: once we have the last exitCode and isRunning props we need to report those
            ((IProgress<(string line, bool isRunning, int exitCode)>)LogProgress).Report((line, true, 0));
        }

        public Task ReportSummary(IEnumerable<SerializableTestCase> tests)
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

        public Task Report(SerializableTestCase test)
            => test.TestOutcome switch
            {
                TestOutcome.Success => Success(test),
                TestOutcome.NotRun => NotRun(test),
                TestOutcome.Ignored => Ignored(test),
                TestOutcome.Failed => Failed(test),
                _ => throw new NotImplementedException($"{nameof(MainWindowViewModel)}.{nameof(Report)}.{nameof(TestOutcome)}={test.TestOutcome}")
            };

        static string GetTestName(SerializableTestCase test)
            => test.FullName;

        private Task Success(SerializableTestCase test)
        {
            WriteLine($"{Scheme.SuccessIcon} {Duration(test)} {GetTestName(test)}");
            if (!string.IsNullOrEmpty(test.AdditionalMessage))
            {
                WriteLine($"\t{test.AdditionalMessage}");
            }
            return Task.CompletedTask;
        }

        private Task NotRun(SerializableTestCase test)
        {
            WriteLine($"{Scheme.NotRunIcon} {Duration(test)} {GetTestName(test)}");
            return Task.CompletedTask;
        }

        private Task Ignored(SerializableTestCase test)
        {
            WriteLine($"{Scheme.IgnoredIcon} {Duration(test)} {GetTestName(test)}");
            if (!string.IsNullOrEmpty(test.IgnoredReason))
            {
                WriteLine($"\t{test.IgnoredReason}");
            }
            return Task.CompletedTask;
        }

        private Task Failed(SerializableTestCase test)
        {
            WriteLine($"{Scheme.ErrorIcon} {Duration(test)} {GetTestName(test)}");
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

        private static string Duration(SerializableTestCase test)
            => test.Duration.AsDuration();

        public void Dispose()
        {
            Commander.Dispose();
        }
    }
}

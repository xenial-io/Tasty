using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Utils;

using static Xenial.Delicious.Utils.Actions;

namespace Xenial.Delicious.Cli
{
    public delegate Task AsyncRemoteTestReporter(SerializableTestCase @case);
    public delegate Task AsyncRemoteTestSummaryReporter(IEnumerable<SerializableTestCase> @cases);

    public class TastyServer
    {
        private readonly List<AsyncRemoteTestReporter> reporters = new List<AsyncRemoteTestReporter>();
        private readonly List<AsyncRemoteTestSummaryReporter> summaryReporters = new List<AsyncRemoteTestSummaryReporter>();

        public TastyServer RegisterReporter(AsyncRemoteTestReporter reporter)
        {
            reporters.Add(reporter);
            return this;
        }

        public TastyServer RegisterReporter(AsyncRemoteTestSummaryReporter reporter)
        {
            summaryReporters.Add(reporter);
            return this;
        }

        public event EventHandler<ExecuteCommandEventArgs>? ExecuteCommand;
        public event EventHandler? CancellationRequested;

        internal async Task DoExecuteCommand(ExecuteCommandEventArgs args)
        {
            ExecuteCommand?.Invoke(this, args);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        internal async Task DoRequestCancellation()
        {
            CancellationRequested?.Invoke(this, EventArgs.Empty);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task Report(SerializableTestCase testCase)
        {
            foreach (var reporter in reporters)
            {
                await reporter.Invoke(testCase).ConfigureAwait(false);
            }
        }

        public async Task Report(IEnumerable<SerializableTestCase> testCases)
        {
            foreach (var reporter in summaryReporters)
            {
                await reporter.Invoke(testCases).ConfigureAwait(false);
            }
        }

        internal Action<IList<SerializableTastyCommand>>? CommandsRegistered;
        internal Action? EndTestPipelineSignaled;
        internal Action? TestPipelineCompletedSignaled;
        public void RegisterCommands(IList<SerializableTastyCommand> commands)
            => CommandsRegistered?.Invoke(commands);

        public void SignalEndTestPipeline()
            => EndTestPipelineSignaled?.Invoke();

        public void SignalTestPipelineCompleted()
            => TestPipelineCompletedSignaled?.Invoke();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Need to be in sync with ITastyRemote")]
        public void ClearConsole()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException) { /* Handle is invalid */}
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Need to be in sync with ITastyRemote")]
        public void ResetColor()
            => Console.ResetColor();

        internal static class ConsoleReporter
        {
            public static ColorScheme Scheme = ColorScheme.Default;

            static ConsoleReporter()
                => Console.OutputEncoding = Encoding.UTF8;

            private static readonly Lazy<int> separatorSize = new Lazy<int>(() =>
            {
                const int fallBackSize = 100;
                try
                {
                    return Console.BufferWidth;
                }
                catch (IOException) { /* Handle is invalid */ }
                return fallBackSize;
            });

            public static Task ReportSummary(IEnumerable<SerializableTestCase> tests)
            {
                var totalTests = tests.Count();
                var failedTests = tests.Count(m => m.TestOutcome == TestOutcome.Failed);
                var ignoredTests = tests.Count(m => m.TestOutcome == TestOutcome.Ignored);
                var notRunTests = tests.Count(m => m.TestOutcome == TestOutcome.NotRun);
                var successTests = tests.Count(m => m.TestOutcome == TestOutcome.Success);
                var outcome = tests.Where(t => t.TestOutcome > TestOutcome.Ignored).Min(t => t.TestOutcome);

                var totalTime = tests.Sum(m => m.Duration);
                var failedTime = tests.Where(m => m.TestOutcome == TestOutcome.Failed).Sum(m => m.Duration);
                var ignoredTime = tests.Where(m => m.TestOutcome == TestOutcome.Ignored).Sum(m => m.Duration);
                var notRunTime = tests.Where(m => m.TestOutcome == TestOutcome.NotRun).Sum(m => m.Duration);
                var successTime = tests.Where(m => m.TestOutcome == TestOutcome.Success).Sum(m => m.Duration);

                var totalTimeString = totalTime.AsDuration();

                Console.WriteLine();
                Console.WriteLine(new string('=', separatorSize.Value));

                Write(Scheme.DefaultColor, "Summary: ");
                Write(failedTests > 0 ? Scheme.ErrorColor : Scheme.DefaultColor, $"F{failedTests}".PadLeft(totalTimeString.Length));
                Write(Scheme.DefaultColor, " | ");
                Write(ignoredTests > 0 ? Scheme.WarningColor : Scheme.DefaultColor, $"I{ignoredTests}".PadLeft(totalTimeString.Length));
                Write(Scheme.DefaultColor, " | ");
                Write(notRunTests > 0 ? Scheme.NotifyColor : Scheme.DefaultColor, $"NR{notRunTests}".PadLeft(totalTimeString.Length));
                Write(Scheme.DefaultColor, " | ");
                Write(successTests > 0 ? Scheme.SuccessColor : Scheme.DefaultColor, $"S{successTests}".PadLeft(totalTimeString.Length));
                Write(Scheme.DefaultColor, " | ");
                Write(Scheme.DefaultColor, $"T{totalTests}");

                Console.WriteLine();
                Write(Scheme.DefaultColor, "Time:    ");
                Write(failedTests > 0 ? Scheme.ErrorColor : Scheme.DefaultColor, failedTime.AsDuration());
                Write(Scheme.DefaultColor, " | ");
                Write(ignoredTests > 0 ? Scheme.WarningColor : Scheme.DefaultColor, ignoredTime.AsDuration());
                Write(Scheme.DefaultColor, " | ");
                Write(notRunTests > 0 ? Scheme.NotifyColor : Scheme.DefaultColor, notRunTime.AsDuration());
                Write(Scheme.DefaultColor, " | ");
                Write(successTests > 0 ? Scheme.SuccessColor : Scheme.DefaultColor, successTime.AsDuration());
                Write(Scheme.DefaultColor, " | ");
                Write(Scheme.DefaultColor, totalTimeString);

                Console.WriteLine();
                Write(Scheme.DefaultColor, "Outcome: ");
                Write(
                    failedTests > 0
                        ? Scheme.ErrorColor
                        : ignoredTests > 0
                            ? Scheme.WarningColor
                            : notRunTests > 0
                            ? Scheme.NotifyColor
                            : Scheme.SuccessColor
                            , outcome.ToString().PadLeft(totalTimeString.Length));

                Console.WriteLine();
                Console.WriteLine(new string('=', separatorSize.Value));
                Console.WriteLine();

                return Task.CompletedTask;
            }

            public static Task Report(SerializableTestCase test)
                => (test ?? throw new ArgumentNullException(nameof(test))).TestOutcome switch
                {
                    TestOutcome.Success => Success(test),
                    TestOutcome.NotRun => NotRun(test),
                    TestOutcome.Ignored => Ignored(test),
                    TestOutcome.Failed => Failed(test),
                    _ => throw new ArgumentOutOfRangeException($"{nameof(ConsoleReporter)}.{nameof(Report)}.{nameof(TestOutcome)}={test.TestOutcome}")
                };

            private static string GetTestName(SerializableTestCase test)
                => test.FullName;

            private static Task Success(SerializableTestCase test)
            {
                WriteLine(Scheme.SuccessColor, $"{Scheme.SuccessIcon} {Duration(test)} {GetTestName(test)}");
                if (!string.IsNullOrEmpty(test.AdditionalMessage))
                {
                    WriteLine(Scheme.SuccessColor, $"\t{test.AdditionalMessage}");
                }
                return Task.CompletedTask;
            }

            private static Task NotRun(SerializableTestCase test)
            {
                WriteLine(Scheme.NotifyColor, $"{Scheme.NotRunIcon} {Duration(test)} {GetTestName(test)}");
                return Task.CompletedTask;
            }

            private static Task Ignored(SerializableTestCase test)
            {
                WriteLine(Scheme.WarningColor, $"{Scheme.IgnoredIcon} {Duration(test)} {GetTestName(test)}");
                if (!string.IsNullOrEmpty(test.IgnoredReason))
                {
                    WriteLine(Scheme.WarningColor, $"\t{test.IgnoredReason}");
                }
                return Task.CompletedTask;
            }

            private static Task Failed(SerializableTestCase test)
            {
                WriteLine(Scheme.ErrorColor, $"{Scheme.ErrorIcon} {Duration(test)} {GetTestName(test)}");
                if (test.Exception != null)
                {
                    WriteLine(Scheme.ErrorColor, $"\t{test.Exception}");
                }
                if (!string.IsNullOrEmpty(test.AdditionalMessage))
                {
                    WriteLine(Scheme.ErrorColor, $"\t{test.AdditionalMessage}");
                }
                return Task.CompletedTask;
            }

            private static void Write(ConsoleColor color, string formattableString)
                => Finally(() =>
                {
                    Console.ForegroundColor = color;
                    Console.Write(formattableString);
                }, ResetColor);

            private static string Duration(SerializableTestCase test)
                => test.Duration.AsDuration();

            private static void WriteLine(ConsoleColor color, string formattableString)
                => Finally(() =>
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine(formattableString);
                }, ResetColor);

            private static void ResetColor()
                => Console.ResetColor();
        }
    }
}

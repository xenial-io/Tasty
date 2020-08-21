using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Utils;

using static Xenial.Delicious.Utils.Actions;

namespace Xenial.Delicious.Reporters
{
    public static class ConsoleReporter
    {
        private static ColorScheme scheme = ColorScheme.Default;
        public static ColorScheme Scheme { get => scheme; set => scheme = value ?? throw new ArgumentNullException(nameof(Scheme)); }
        public static TastyScope RegisterConsoleReporter(this TastyScope scope)
            => (scope ?? throw new ArgumentNullException(nameof(scope))).RegisterReporter(Report)
                    .RegisterReporter(ReportSummary);

        public static TastyScope Register()
            => Tasty.TastyDefaultScope.RegisterConsoleReporter();

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

        public static Task ReportSummary(IEnumerable<TestCaseResult> tests)
        {
            var totalTests = tests.Count();
            var failedTests = tests.Where(m => m.TestOutcome == TestOutcome.Failed).Count();
            var ignoredTests = tests.Where(m => m.TestOutcome == TestOutcome.Ignored).Count();
            var notRunTests = tests.Where(m => m.TestOutcome == TestOutcome.NotRun).Count();
            var successTests = tests.Where(m => m.TestOutcome == TestOutcome.Success).Count();
            var outcome = tests.Where(t => t.TestOutcome > TestOutcome.Ignored).MinOrDefault(t => t.TestOutcome);

            var totalTime = tests.Sum(m => m.Duration);
            var failedTime = tests.Where(m => m.TestOutcome == TestOutcome.Failed).Sum(m => m.Duration);
            var ignoredTime = tests.Where(m => m.TestOutcome == TestOutcome.Ignored).Sum(m => m.Duration);
            var notRunTime = tests.Where(m => m.TestOutcome == TestOutcome.NotRun).Sum(m => m.Duration);
            var successTime = tests.Where(m => m.TestOutcome == TestOutcome.Success).Sum(m => m.Duration);

            var totalTimeString = totalTime.AsDuration();

            Console.WriteLine();
            Console.WriteLine(new string('=', separatorSize.Value));

            Write(Scheme.DefaultColor, $"Summary: ");
            Write(failedTests > 0 ? Scheme.ErrorColor : Scheme.DefaultColor, $"F{failedTests}".PadLeft(totalTimeString.Length));
            Write(Scheme.DefaultColor, $" | ");
            Write(ignoredTests > 0 ? Scheme.WarningColor : Scheme.DefaultColor, $"I{ignoredTests}".PadLeft(totalTimeString.Length));
            Write(Scheme.DefaultColor, $" | ");
            Write(notRunTests > 0 ? Scheme.NotifyColor : Scheme.DefaultColor, $"NR{notRunTests}".PadLeft(totalTimeString.Length));
            Write(Scheme.DefaultColor, $" | ");
            Write(successTests > 0 ? Scheme.SuccessColor : Scheme.DefaultColor, $"S{successTests}".PadLeft(totalTimeString.Length));
            Write(Scheme.DefaultColor, $" | ");
            Write(Scheme.DefaultColor, $"T{totalTests}");

            Console.WriteLine();
            Write(Scheme.DefaultColor, $"Time:    ");
            Write(failedTests > 0 ? Scheme.ErrorColor : Scheme.DefaultColor, failedTime.AsDuration());
            Write(Scheme.DefaultColor, $" | ");
            Write(ignoredTests > 0 ? Scheme.WarningColor : Scheme.DefaultColor, ignoredTime.AsDuration());
            Write(Scheme.DefaultColor, $" | ");
            Write(notRunTests > 0 ? Scheme.NotifyColor : Scheme.DefaultColor, notRunTime.AsDuration());
            Write(Scheme.DefaultColor, $" | ");
            Write(successTests > 0 ? Scheme.SuccessColor : Scheme.DefaultColor, successTime.AsDuration());
            Write(Scheme.DefaultColor, $" | ");
            Write(Scheme.DefaultColor, totalTimeString);

            Console.WriteLine();
            Write(Scheme.DefaultColor, $"Outcome: ");
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

        public static Task Report(TestCaseResult test)
            => (test ?? throw new ArgumentNullException(nameof(test))).TestOutcome switch
            {
                TestOutcome.Success => Success(test),
                TestOutcome.NotRun => NotRun(test),
                TestOutcome.Ignored => Ignored(test),
                TestOutcome.Failed => Failed(test),
                _ => throw new NotImplementedException($"{nameof(ConsoleReporter)}.{nameof(Report)}.{nameof(TestOutcome)}={test.TestOutcome}")
            };

        private static string GetTestName(TestCaseResult test)
            => test.FullName;

        private static Task Success(TestCaseResult test)
        {
            WriteLine(Scheme.SuccessColor, $"{Scheme.SuccessIcon} {Duration(test)} {GetTestName(test)}");
            if (!string.IsNullOrEmpty(test.AdditionalMessage))
            {
                WriteLine(Scheme.SuccessColor, $"\t{test.AdditionalMessage}");
            }
            return Task.CompletedTask;
        }

        private static Task NotRun(TestCaseResult test)
        {
            WriteLine(Scheme.NotifyColor, $"{Scheme.NotRunIcon} {Duration(test)} {GetTestName(test)}");
            return Task.CompletedTask;
        }

        private static Task Ignored(TestCaseResult test)
        {
            WriteLine(Scheme.WarningColor, $"{Scheme.IgnoredIcon} {Duration(test)} {GetTestName(test)}");
            if (!string.IsNullOrEmpty(test.IgnoredReason))
            {
                WriteLine(Scheme.WarningColor, $"\t{test.IgnoredReason}");
            }
            return Task.CompletedTask;
        }

        private static Task Failed(TestCaseResult test)
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

        private static string Duration(TestCaseResult test)
            => test.Duration.AsDuration();

        private static void WriteLine(ConsoleColor color, string formattableString)
            => Finally(() =>
            {
                Console.ForegroundColor = color;
                Console.WriteLine(formattableString);
            }, ResetColor);

        private static void Write(ConsoleColor color, string formattableString)
            => Finally(() =>
            {
                Console.ForegroundColor = color;
                Console.Write(formattableString);
            }, ResetColor);

        private static void ResetColor()
            => Console.ResetColor();
    }
}

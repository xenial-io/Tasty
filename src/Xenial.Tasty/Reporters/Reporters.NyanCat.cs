using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Reporters
{
    public static class NyanCatReporter
    {
        private const int ColorCount = 6 * 7;
        private static readonly int nyanCatWidth;
        private static readonly int width;
        private static int colorIndex;
        private static readonly int numberOfLines;
        private static readonly Color[]? rainbowColors;
        private static bool tick;
        private static readonly List<List<string>>? trajectories;
        private static readonly int trajectoryWidthMax;

        static NyanCatReporter()
        {
            if (!HasValidConsole)
                return;

            Pastel.ConsoleExtensions.Enable();

            nyanCatWidth = 11;
            width = (int)(Console.WindowWidth * 0.75);

            colorIndex = 0;
            numberOfLines = Enum.GetValues(typeof(TestOutcome)).Length;

            rainbowColors = GenerateColors();
            tick = false;

            trajectories = new List<List<string>>();

            for (int i = 0; i < numberOfLines; i++)
            {
                trajectories.Add(new List<string>());
            }

            trajectoryWidthMax = width - nyanCatWidth;

            if (trajectoryWidthMax < 0)
                trajectoryWidthMax = 0;
        }

        public static void Register()
            => Tasty.RegisterReporter(Report)
                    .RegisterReporter(ReportSummary);

        static bool? _HasValidConsole;
        private static bool HasValidConsole
        {
            get
            {
                if (!_HasValidConsole.HasValue)
                {
                    try
                    {
                        _HasValidConsole = Console.WindowWidth > 0;
                    }
                    catch
                    {
                        _HasValidConsole = false;
                    }
                }

                return _HasValidConsole.Value;
            }
        }

        private static Task ReportSummary(TestSummary summary)
        {
            Console.WriteLine();
            Console.ForegroundColor = ColorScheme.Default.DefaultColor;
            Console.WriteLine($"\t{summary.Count()} total ({(int)Math.Round(TimeSpan.FromTicks(summary.Sum(t => t.Duration.Ticks)).TotalSeconds, 0, MidpointRounding.AwayFromZero)} s)");

            Console.ForegroundColor = ColorScheme.Default.SuccessColor;
            var successCount = summary.Count(t => t.TestOutcome == TestOutcome.Success);
            if (successCount > 0) { Console.WriteLine($"\t{ColorScheme.Default.SuccessIcon} {successCount} passing"); }

            Console.ForegroundColor = ColorScheme.Default.ErrorColor;
            var failedCount = summary.Count(t => t.TestOutcome == TestOutcome.Failed);
            if (failedCount > 0) { Console.WriteLine($"\t{ColorScheme.Default.ErrorIcon} {failedCount} failed"); }

            Console.ForegroundColor = ColorScheme.Default.SuccessColor;
            if (summary.Count() == successCount) { Console.WriteLine($"\t{ColorScheme.Default.SuccessIcon} All tests passed"); }

            if (failedCount > 0)
            {
                Console.ForegroundColor = ColorScheme.Default.ErrorColor;
                Console.WriteLine($"\t{ColorScheme.Default.ErrorIcon} Failed tests:");

                foreach (var fail in summary.Where(t => t.TestOutcome == TestOutcome.Failed))
                {
                    Console.WriteLine($"\t\t{fail.FullName}");
                    Console.WriteLine($"\t\t\t{fail.AdditionalMessage}");
                }
            }

            return Task.CompletedTask;
        }

        public static Task Report(TestCase test)
        {
            if (!HasValidConsole)
            {
                return Task.CompletedTask;
            }

            Draw(test);

            return Task.CompletedTask;
        }

        private static Color[] GenerateColors()
        {
            var colors = new Color[ColorCount];
            var progress = 0f;
            var step = 1f / ColorCount;

            for (var i = 0; i < ColorCount; i++)
            {
                colors[i] = Rainbow(progress);

                progress += step;
            }

            return colors;
        }

        private static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            return ((int)div) switch
            {
                0 => Color.FromArgb(255, 255, ascending, 0),
                1 => Color.FromArgb(255, descending, 255, 0),
                2 => Color.FromArgb(255, 0, 255, ascending),
                3 => Color.FromArgb(255, 0, descending, 255),
                4 => Color.FromArgb(255, ascending, 0, 255),
                _ => Color.FromArgb(255, 255, 0, descending),
            };
        }

        private static void Draw(TestCase testCase)
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;

            tick = !tick;

            for (int i = 0; i < numberOfLines; i++)
            {
                if (trajectories![i].Count > trajectoryWidthMax)
                {
                    foreach (var traj in trajectories)
                    {
                        traj.RemoveAt(0);
                    }
                }

                trajectories[i].Add(AppendRainbow());

            }

            var catIndex = 0;
            var cat = GetNyanCat(testCase).ToList();

            foreach (var traj in trajectories!)
            {
                Console.WriteLine(string.Join(string.Empty, traj) + cat[catIndex]);
                catIndex++;
            }
        }

        private static string AppendRainbow() => Rainbowify(tick ? "_" : "-");

        private static string Rainbowify(string input)
        {
            var color = rainbowColors![colorIndex % rainbowColors.Length];

            var result = Pastel.ConsoleExtensions.Pastel(input, color);

            colorIndex += 1;

            return result;
        }

        private static IEnumerable<string> GetNyanCat(TestCase testCase)
        {
            yield return " _,------,";
            yield return " _|   /\\_/\\";
            yield return " ^|__" + Face(testCase);
            yield return "   \"\"  \"\" ";
        }

        private static string Face(TestCase testCase)
            => testCase.TestOutcome switch
            {
                TestOutcome.NotRun => "( o .o)",
                TestOutcome.Ignored => "( - .-)",
                TestOutcome.Failed => "( x .x)",
                TestOutcome.Success => "( ^ .^)",
                _ => "( - .-)",
            };
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Reporters
{
    public static class NyanCatReporter
    {
        private const int ColorCount = 6 * 7;
        private static int nyanCatWidth;
        private static int width;
        private static int colorIndex;
        private static int numberOfLines;
        private static Color[] rainbowColors;
        //private static int scoreboardWidth;
        private static bool tick;
        private static List<List<string>> trajectories;
        private static int trajectoryWidthMax;

        static NyanCatReporter()
        {
            Pastel.ConsoleExtensions.Enable();

            nyanCatWidth = 11;
            width = (int)(Console.WindowWidth * 0.75);

            colorIndex = 0;
            numberOfLines = 4;

            rainbowColors = GenerateColors();
            //scoreboardWidth = 5;
            tick = false;

            trajectories = new List<List<string>>();

            for (int i = 0; i < numberOfLines; i++)
            {
                trajectories.Add(new List<string>());
            }

            trajectoryWidthMax = width - nyanCatWidth;
        }

        public static void Register()
        {
            Tasty.RegisterReporter(Report)
                 .RegisterReporter(ReportSummary);
        }

        private static Task ReportSummary(IEnumerable<TestCase> tests)
        {
            //foreach (var test in tests)
            //    Draw(test);

            return Task.CompletedTask;
        }

        public static Task Report(TestCase test)
        {
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

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, ascending, 0);
                case 1:
                    return Color.FromArgb(255, descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, ascending);
                case 3:
                    return Color.FromArgb(255, 0, descending, 255);
                case 4:
                    return Color.FromArgb(255, ascending, 0, 255);
                default:
                    return Color.FromArgb(255, 255, 0, descending);
            }
        }

        private static void Draw(TestCase testCase)
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;

            tick = !tick;

            for (int i = 0; i < numberOfLines; i++)
            {
                if (trajectories[i].Count > trajectoryWidthMax)
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

            foreach (var traj in trajectories)
            {
                Console.WriteLine(string.Join("", traj) + cat[catIndex]);
                catIndex++;
            }
        }

        private static string AppendRainbow()
        {
            return Rainbowify(tick ? "_" : "-");
        }

        private static string Rainbowify(string input)
        {
            var color = rainbowColors[colorIndex % rainbowColors.Length];

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
        {
            switch (testCase.TestOutcome)
            {
                case TestOutcome.NotRun:
                    return "( o .o)";
                case TestOutcome.Ignored:
                    return "( - .-)";
                case TestOutcome.Failed:
                    return "( x .x)";
                case TestOutcome.Success:
                    return "( ^ .^)";
            }

            return "( - .-)";
        }
    }
}

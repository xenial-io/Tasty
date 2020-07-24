using System;
using System.Collections.Generic;
using System.Drawing;
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
        private static int scoreboardWidth;
        private static bool tick;
        private static List<StringBuilder> trajectories;
        private static int trajectoryWidthMax;

        static NyanCatReporter()
        {
            Pastel.ConsoleExtensions.Enable();

            nyanCatWidth = 11;
            width = (int)(Console.WindowWidth * 0.75);

            colorIndex = 0;
            numberOfLines = 4;

            rainbowColors = GenerateColors();
            scoreboardWidth = 5;
            tick = false;

            trajectories = new List<StringBuilder>();

            for (int i = 0; i < numberOfLines; i++)
            {
                trajectories.Add(new StringBuilder());
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
            foreach (var test in tests)
                Draw(test);

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
            for (int i = 0; i < numberOfLines; i++)
            {
                if (trajectories[i].Length < trajectoryWidthMax)
                {
                    tick = !tick;
                    trajectories[i].Append(AppendRainbow());
                }
            }

            AppendNyanCat(testCase);

            foreach (var traj in trajectories)
            {
                Console.WriteLine(traj.ToString());
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

        private static void AppendNyanCat(TestCase testCase)
        {
            trajectories[0].Append(" _,------,");
            trajectories[1].Append(" _|   /\\_/\\");
            trajectories[2].Append(" ^|__" + Face(testCase));
            trajectories[3].Append("   \"\"  \"\" ");
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

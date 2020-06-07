using System;

namespace Xenial.Delicious.Reporters
{
    public class ColorScheme
    {
        public ConsoleColor DefaultColor = ConsoleColor.White;
        public ConsoleColor ErrorColor = ConsoleColor.Red;
        public ConsoleColor WarningColor = ConsoleColor.Yellow;
        public ConsoleColor NotifyColor = ConsoleColor.DarkGray;
        public ConsoleColor SuccessColor = ConsoleColor.DarkGreen;

        public string ErrorIcon = "👎";
        public string SuccessIcon = "👍";
        public string NotRunIcon = "🙈";
        public string IgnoredIcon = "🙄";

        public static ColorScheme Default = new ColorScheme();
    }
}

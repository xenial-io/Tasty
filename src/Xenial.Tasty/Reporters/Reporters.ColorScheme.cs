using System;

using static Xenial.Delicious.FeatureDetection.FeatureDetector;

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

        public static ColorScheme Default = SupportsRichContent() 
            ? new ColorScheme()
            : new ColorSchemeLegacy();
    }

    public class ColorSchemeLegacy : ColorScheme
    {
        public ColorSchemeLegacy()
        {
            SuccessIcon = "☺";
            ErrorIcon = "▼";
            NotRunIcon = "?";
            IgnoredIcon = "‼";
        }

        public static new ColorScheme Default = new ColorSchemeLegacy();
    }
}

using System;
using System.Runtime.InteropServices;
using System.Text;

using static Xenial.Delicious.FeatureDetection.FeatureDetector;

namespace Xenial.Delicious.Reporters
{
    public class ColorScheme
    {
        static ColorScheme()
            => SetupConsoleEncoding();

        private static void SetupConsoleEncoding()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //Register additional code pages for windows
                //cause we deal directly with process streams
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }

        public ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor NotifyColor { get; set; } = ConsoleColor.DarkGray;
        public ConsoleColor SuccessColor { get; set; } = ConsoleColor.DarkGreen;

        public string ErrorIcon { get; set; }= "👎";
        public string SuccessIcon { get; set; } = "👍";
        public string NotRunIcon { get; set; } = "🙈";
        public string IgnoredIcon { get; set; } = "🙄";

        public static ColorScheme Default => SupportsRichContent()
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

        public new static ColorScheme Default => new ColorSchemeLegacy();
    }
}

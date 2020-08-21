using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xenial.Delicious.FeatureDetection
{
    internal static class FeatureDetector
    {
        internal static readonly string[] CmdProcessNames = new[]
        {
            "cmd",
            "VsDebugConsole",
            "powershell"
        };

        internal static bool IsWindows()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        internal static bool IsWindowsTerminal()
        {
            var env = Environment.GetEnvironmentVariable("WT_SESSION");
            if (!string.IsNullOrEmpty(env) && Guid.TryParse(env, out _))
            {
                return true;
            }
            return false;
        }

        internal static bool SupportsRichContent()
        {
            if (!IsWindows())
            {
                return true;
            }

            if (IsWindowsTerminal())
            {
                return true;
            }

            return false;
        }
    }
}

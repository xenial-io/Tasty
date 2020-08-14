using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Xenial.Delicious.FeatureDetection.Win;

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

        public static bool IsCmd()
        {
            var proc = Process.GetCurrentProcess();

            bool IsCmdProc(Process process)
            {
                if (process != null)
                {
                    if (CmdProcessNames.Any(name => string.Equals(name, process.ProcessName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        return true;
                    }
                }
                return false;
            }
            while (proc != null)
            {
                if (IsCmdProc(proc))
                {
                    return true;
                }
                proc = proc.Parent();
            }
            return false;
        }

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

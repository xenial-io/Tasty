using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xenial.Delicious.FeatureDetection.Win
{
    internal static class ProcessExtensions
    {
        internal static Process Parent(this Process p)
        {
            var entries = Toolhelp32.TakeSnapshot<WinProcessEntry>(Toolhelp32.SnapAll, 0);
            var parentid = entries.First(x => x.th32ProcessID == p.Id).th32ParentProcessID;
            return Process.GetProcessById(parentid);
        }
    }
}

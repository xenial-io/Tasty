using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xenial.Delicious.FeatureDetection.Win
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct WinProcessEntry : Toolhelp32.IEntry
    {
        [DllImport("kernel32.dll")]
        internal static extern bool Process32Next(Toolhelp32.Snapshot snap, ref WinProcessEntry entry);

        bool Toolhelp32.IEntry.TryMoveNext(Toolhelp32.Snapshot snap, out Toolhelp32.IEntry entry)
        {
            var x = new WinProcessEntry { dwSize = Marshal.SizeOf(typeof(WinProcessEntry)) };
            var b = Process32Next(snap, ref x);
            entry = x;
            return b;
        }

        internal int dwSize;
        internal int cntUsage;
        internal int th32ProcessID;
        internal IntPtr th32DefaultHeapID;
        internal int th32ModuleID;
        internal int cntThreads;
        internal int th32ParentProcessID;
        internal int pcPriClassBase;
        internal int dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        internal string fileName;
    }
}

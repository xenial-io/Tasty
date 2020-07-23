using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xenial.Delicious.FeatureDetection.Win
{
    internal static class Toolhelp32
    {
        internal const uint Inherit = 0x80000000;
        internal const uint SnapModule32 = 0x00000010;
        internal const uint SnapAll = SnapHeapList | SnapModule | SnapProcess | SnapThread;
        internal const uint SnapHeapList = 0x00000001;
        internal const uint SnapProcess = 0x00000002;
        internal const uint SnapThread = 0x00000004;
        internal const uint SnapModule = 0x00000008;

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateToolhelp32Snapshot(uint flags, int processId);

        internal static IEnumerable<T> TakeSnapshot<T>(uint flags, int id) where T : IEntry, new()
        {
            using (var snap = new Snapshot(flags, id))
                for (IEntry entry = new T { }; entry.TryMoveNext(snap, out entry);)
                    yield return (T)entry;
        }

        internal interface IEntry
        {
            internal bool TryMoveNext(Snapshot snap, out IEntry entry);
        }

        internal struct Snapshot : IDisposable
        {
            void IDisposable.Dispose()
            {
                CloseHandle(m_handle);
            }
            internal Snapshot(uint flags, int processId)
            {
                m_handle = CreateToolhelp32Snapshot(flags, processId);
            }
            IntPtr m_handle;
        }
    }
}

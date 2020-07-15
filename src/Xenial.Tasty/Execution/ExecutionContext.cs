using System;
using System.IO;
using System.Linq;

using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Execution
{
    public class RuntimeContext : IDisposable
    {
        private bool disposedValue;

        public RuntimeContext(TastyScope scope)
            => Scope = scope ?? throw new ArgumentNullException(nameof(scope));

        public TastyScope Scope { get; }
        public bool IsRemoteAttached { get; set; }
        public Stream? RemoteStream { get; set; }
        public TastyRemote? Remote { get; set; }
        public int ExitCode { get; internal set; }
        public bool IsInteractive { get; internal set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RemoteStream?.Dispose();
                    Remote?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

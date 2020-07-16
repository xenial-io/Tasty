using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Execution
{
    public class RuntimeContext : IDisposable
    {
        private bool disposedValue;

        public RuntimeContext(TastyScope scope, TestExecutor executor)
            => (Scope, Executor) = (
                scope ?? throw new ArgumentNullException(nameof(scope)),
                executor ?? throw new ArgumentNullException(nameof(executor))
            );

        public TastyScope Scope { get; }
        public TestExecutor Executor { get; }
        public bool IsRemoteAttached => RemoteStream != null;
        public Stream? RemoteStream { get; set; }
        public TastyRemote? Remote { get; set; }
        public int ExitCode { get; internal set; }
        public bool IsInteractive { get; internal set; }
        public bool EndPipeLine { get; set; }
        public Queue<TestCase> TestQueue { get; set; } = new Queue<TestCase>();
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

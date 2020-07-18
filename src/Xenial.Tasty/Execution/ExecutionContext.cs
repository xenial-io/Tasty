using System;
using System.Collections.Generic;
using System.IO;

using Xenial.Delicious.Commands;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Execution
{
    public class RuntimeContext : IDisposable
    {
        public RuntimeContext(TestExecutor executor)
            => Executor = executor ?? throw new ArgumentNullException(nameof(executor));

        public TestExecutor Executor { get; }
        public TastyScope Scope => Executor.Scope;
        public bool IsRemoteAttached => RemoteStream != null;
        public Stream? RemoteStream { get; internal set; }
        public TastyRemote? Remote { get; internal set; }
        public int ExitCode { get; internal set; }
        public bool IsInteractive { get; internal set; }
        public bool EndPipeLine { get; internal set; }
        public Queue<TestCase> TestQueue { get; set; } = new Queue<TestCase>();
        public TastyCommand? CurrentCommand { get; set; }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RemoteStream?.Dispose();
                    Remote?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Protocols;

namespace Xenial.Delicious.Scopes
{
    public interface TastyRemote : IDisposable
    {
        public event EventHandler<ExecuteCommandEventArgs>? ExecuteCommand;
        public event EventHandler? CancellationRequested;
        public event EventHandler? Exit;
        Task ClearConsole();
        Task Report(SerializableTestCase @case);
    }
}
using System;
using System.Collections.Generic;
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
        Task ResetColor();
        Task Report(SerializableTestCase @case);
        Task Report(IEnumerable<SerializableTestCase> @cases);
        Task RegisterCommands(IList<SerializableTastyCommand> commands);
        Task SignalEndTestPipeline();
        Task SignalTestPipelineCompleted();
    }
}
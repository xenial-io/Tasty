using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xenial.Delicious.Protocols;

namespace Xenial.Delicious.Scopes
{
    public interface ITastyRemote : IDisposable
    {
        public event EventHandler<ExecuteCommandEventArgs>? ExecuteCommand;
        public event EventHandler? CancellationRequested;
        Task ClearConsole();
        Task ResetColor();
        Task Report(SerializableTestCase testCase);
        Task Report(IEnumerable<SerializableTestCase> testCases);
        Task RegisterCommands(IList<SerializableTastyCommand> commands);
        Task SignalEndTestPipeline();
        Task SignalTestPipelineCompleted();
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xenial.Delicious.Protocols;
using Xenial.Delicious.Reporters;

namespace Xenial.Delicious.Protocols
{
    public interface ITastyRemote : IDisposable
    {
        public event EventHandler<ExecuteCommandEventArgs>? ExecuteCommand;
        public event EventHandler? CancellationRequested;
        Task ClearConsole();
        Task ResetColor();
        Task Report(TestCaseResult testCase);
        Task Report(IEnumerable<TestCaseResult> testCases);
        Task RegisterCommands(IList<SerializableTastyCommand> commands);
        Task SignalEndTestPipeline();
        Task SignalTestPipelineCompleted();
    }
}

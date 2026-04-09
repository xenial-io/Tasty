using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyType;
using StreamJsonRpc;
using Xenial.Delicious.Reporters;

namespace Xenial.Delicious.Protocols
{
    [JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
    public partial interface ITastyRemote : IDisposable
    {
        event EventHandler<ExecuteCommandEventArgs>? ExecuteCommand;
        event EventHandler? CancellationRequested;
        Task ClearConsole();
        Task ResetColor();
        Task Report(TestCaseResult testCase);
        Task Report(IEnumerable<TestCaseResult> testCases);
        Task RegisterCommands(IList<SerializableTastyCommand> commands);
        Task SignalEndTestPipeline();
        Task SignalTestPipelineCompleted();
    }
}

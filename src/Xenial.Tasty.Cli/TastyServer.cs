using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Protocols;
using Xenial.Delicious.Reporters;
using Xenial.Delicious.Utils;

using static Xenial.Delicious.Utils.Actions;

namespace Xenial.Delicious.Cli
{
    public class TastyServer
    {
        private readonly List<AsyncTestReporter> reporters = new List<AsyncTestReporter>();
        private readonly List<AsyncTestSummaryReporter> summaryReporters = new List<AsyncTestSummaryReporter>();

        public TastyServer RegisterReporter(AsyncTestReporter reporter)
        {
            reporters.Add(reporter);
            return this;
        }

        public TastyServer RegisterReporter(AsyncTestSummaryReporter reporter)
        {
            summaryReporters.Add(reporter);
            return this;
        }

        public event EventHandler<ExecuteCommandEventArgs>? ExecuteCommand;
        public event EventHandler? CancellationRequested;

        internal async Task DoExecuteCommand(ExecuteCommandEventArgs args)
        {
            ExecuteCommand?.Invoke(this, args);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        internal async Task DoRequestCancellation()
        {
            CancellationRequested?.Invoke(this, EventArgs.Empty);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task Report(TestCaseResult testCase)
        {
            foreach (var reporter in reporters)
            {
                await reporter.Invoke(testCase).ConfigureAwait(false);
            }
        }

        public async Task Report(IEnumerable<TestCaseResult> testCases)
        {
            foreach (var reporter in summaryReporters)
            {
                await reporter.Invoke(testCases).ConfigureAwait(false);
            }
        }

        internal Action<IList<SerializableTastyCommand>>? CommandsRegistered;
        internal Action? EndTestPipelineSignaled;
        internal Action? TestPipelineCompletedSignaled;
        public void RegisterCommands(IList<SerializableTastyCommand> commands)
            => CommandsRegistered?.Invoke(commands);

        public void SignalEndTestPipeline()
            => EndTestPipelineSignaled?.Invoke();

        public void SignalTestPipelineCompleted()
            => TestPipelineCompletedSignaled?.Invoke();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Need to be in sync with ITastyRemote")]
        public void ClearConsole()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException) { /* Handle is invalid */}
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Need to be in sync with ITastyRemote")]
        public void ResetColor()
            => Console.ResetColor();
    }
}

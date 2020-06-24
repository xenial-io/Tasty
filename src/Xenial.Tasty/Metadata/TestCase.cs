using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Metadata
{

    [System.Diagnostics.DebuggerDisplay("Name: {Name} Outcome: {TestOutcome} Duration: {Duration}")]
    public class TestCase : IExecutable, IForceAble
    {
        public string Name { get; internal set; } = string.Empty;
        public TestGroup? Group { get; internal set; }
        public Executable Executor { get; set; } = () => Task.FromResult(true);
        public TestOutcome TestOutcome { get; internal set; } = TestOutcome.NotRun;
        public Exception? Exception { get; internal set; }
        public Func<bool?> IsIgnored { get; internal set; } = () => false;
        public string IgnoredReason { get; internal set; } = string.Empty;
        public TimeSpan Duration { get; internal set; }
        public string AdditionalMessage { get; internal set; } = string.Empty;
        public Func<bool>? IsForced { get; set; }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Metadata
{

    [System.Diagnostics.DebuggerDisplay("Name: {Name} Outcome: {TestOutcome} Duration: {Duration}")]
    public class TestCase : IExecutable, IForceAble
    {
        public string Name { get; internal set; }
        public TestGroup Group { get; internal set; }
        public Executable Executor { get; set; }
        public TestOutcome TestOutcome { get; internal set; } = TestOutcome.NotRun;
        public Exception Exception { get; internal set; }
        public Func<bool?> IsIgnored { get; internal set; }
        public Func<bool?> IsInconclusive { get; internal set; }
        public string IgnoredReason { get; internal set; }
        public string InconclusiveReason { get; internal set; }
        public TimeSpan Duration { get; internal set; }
        public string AdditionalMessage { get; internal set; }
        public Func<bool> IsForced { get; internal set; }
    }
}

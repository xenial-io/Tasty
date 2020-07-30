using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;

namespace Xenial.Delicious.Metadata
{

    [System.Diagnostics.DebuggerDisplay("Name: {Name} Outcome: {TestOutcome} Duration: {Duration}")]
    public class TestCase : IExecutable, IForceAble
    {
        private TestOutcome _Testoutcome = TestOutcome.NotRun;

        public string Name { get; internal set; } = string.Empty;
        public TestGroup? Group { get; internal set; }
        public Executable Executor { get; set; } = () => Task.FromResult(true);
        
        public TestOutcome TestOutcome
        {
            get => _Testoutcome; internal set
            {
                _Testoutcome = value;
                if (_Testoutcome == TestOutcome.Failed) { Debugger.Break(); }
            }
        }

        public Exception? Exception { get; internal set; }
        public Func<bool?> IsIgnored { get; internal set; } = () => false;
        public string IgnoredReason { get; internal set; } = string.Empty;
        public TimeSpan Duration { get; internal set; }
        public string AdditionalMessage { get; internal set; } = string.Empty;
        public Func<bool>? IsForced { get; set; }

        public string FullName =>
            Group == null
            ? Name
            : $"{Group.FullName} {Name}";
    }
}

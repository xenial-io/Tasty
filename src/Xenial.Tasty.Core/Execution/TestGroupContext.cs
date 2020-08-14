using System;
using System.Collections.Generic;
using System.Linq;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Execution
{
    public class TestGroupContext
    {
        public TestGroupContext(
            TestGroup currentGroup,
            TastyScope currentScope,
            Queue<TestGroup> groupQueue,
            Queue<TestCase> testQueue
            )
        {
            CurrentGroup = currentGroup ?? throw new ArgumentNullException(nameof(currentGroup));
            CurrentScope = currentScope ?? throw new ArgumentNullException(nameof(currentScope));
            GroupQueue = groupQueue ?? throw new ArgumentNullException(nameof(groupQueue));
            TestQueue = testQueue ?? throw new ArgumentNullException(nameof(testQueue));
        }

        public TestGroup CurrentGroup { get; }
        public TastyScope CurrentScope { get; }
        public Queue<TestGroup> GroupQueue { get; }
        public Queue<TestCase> TestQueue { get; }
    }
}

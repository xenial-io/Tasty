using System;
using System.Linq;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Execution
{
    public class TestExecutionContext
    {
        public TestExecutionContext(TestCase currentCase, TastyScope currentScope, TestGroup? currentGroup)
        {
            CurrentCase = currentCase ?? throw new ArgumentNullException(nameof(currentCase));
            CurrentScope = currentScope ?? throw new ArgumentNullException(nameof(currentScope));
            CurrentGroup = currentGroup;
        }

        public TestCase CurrentCase { get; }
        public TastyScope CurrentScope { get; }
        public TestGroup? CurrentGroup { get; }
    }

    public class TestGroupContext
    {
        public TestGroupContext(TestGroup currentGroup, TastyScope currentScope)
        {
            CurrentGroup = currentGroup ?? throw new ArgumentNullException(nameof(currentGroup));
            CurrentScope = currentScope ?? throw new ArgumentNullException(nameof(currentScope));
        }

        public TestGroup CurrentGroup { get; }
        public TastyScope CurrentScope { get; }
    }
}

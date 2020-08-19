
using System;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Metadata
{
    public class TestHook : IExecutable
    {
        public TestHook(Executable executor, TestGroup? group)
        {
            Executor = executor ?? throw new ArgumentNullException(nameof(executor));
            Group = group;
        }

        public Executable Executor { get; }
        public TestGroup? Group { get; internal set; }
    }

    public class TestBeforeEachHook : TestHook
    {
        public TestBeforeEachHook(Executable executor, TestGroup? group)
            : base(executor, group) { }
    }

    public class TestAfterEachHook : TestHook
    {
        public TestAfterEachHook(Executable executor, TestGroup? group)
            : base(executor, group) { }
    }
}

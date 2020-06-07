
using System;
using System.Threading.Tasks;

using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Metadata
{
    public class TestHook : IExecutable
    {
        public Func<Task<bool>> Executor { get; internal set; }
        public TestGroup Group { get; internal set; }
    }

    public class TestBeforeEachHook : TestHook { }

    public class TestAfterEachHook : TestHook { }
}
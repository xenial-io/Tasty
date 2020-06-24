using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution.TestGroupMiddleware;
using Xenial.Delicious.Execution.TestMiddleware;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Visitors;

namespace Xenial.Delicious.Execution
{
    public class TestExecutor
    {
        private IList<Func<TestDelegate, TestDelegate>> TestMiddlewares = new List<Func<TestDelegate, TestDelegate>>();
        private IList<Func<TestGroupDelegate, TestGroupDelegate>> TestGroupMiddlewares = new List<Func<TestGroupDelegate, TestGroupDelegate>>();
        internal TastyScope Scope { get; }

        public TestExecutor(TastyScope scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));

            this
                .UseTestGroupReporters()
                .UseTestGroupStopwatch()
                .UseTestGroupScope()
                .UseTestGroupExecutor()
                .UseTestGroupCollector()
                .UseTestCaseCollector()
                ;

            this
                .UseTestReporters()
                .UseTestStopwatch()
                .UseForcedTestExecutor()
                .UseIgnoreTestExecutor()
                .UseBeforeEachTest()
                .UseTestExecutor()
                .UseAfterEachTest()
                ;
        }

        public TestExecutor Use(Func<TestDelegate, TestDelegate> middleware)
        {
            TestMiddlewares.Add(middleware);
            return this;
        }

        public TestExecutor Use(Func<TestGroupDelegate, TestGroupDelegate> middleware)
        {
            TestGroupMiddlewares.Add(middleware);
            return this;
        }

        public async Task Execute()
        {
            var groupQueue = new Queue<TestGroup>(Scope.Descendants().OfType<TestGroup>());
            var testQueue = new Queue<TestCase>();

            while (groupQueue.Count > 0)
            {
                var currentGroup = groupQueue.Dequeue();
                await Execute(groupQueue, testQueue, currentGroup);
            }

            while (testQueue.Count > 0)
            {
                var currentTest = testQueue.Dequeue();
                await Execute(currentTest);
            }
        }

        internal async Task Execute(Queue<TestGroup> groupQueue, Queue<TestCase> testQueue, TestGroup testGroup)
        {
            var app = BuildTestGroupMiddleware();
            var context = new TestGroupContext(testGroup, Scope, groupQueue, testQueue);
            await app(context);
        }

        internal async Task Execute(TestCase testCase)
        {
            var app = BuildTestMiddleware();
            var context = new TestExecutionContext(testCase, Scope, testCase.Group);
            await app(context);
        }

        internal TestDelegate BuildTestMiddleware()
        {
            TestDelegate app = context =>
            {
                return Task.CompletedTask;
            };

            foreach (var middleware in TestMiddlewares.Reverse())
            {
                app = middleware(app);
            }

            return app;
        }

        internal TestGroupDelegate BuildTestGroupMiddleware()
        {
            TestGroupDelegate app = context =>
            {
                return Task.CompletedTask;
            };

            foreach (var middleware in TestGroupMiddlewares.Reverse())
            {
                app = middleware(app);
            }

            return app;
        }
    }
}

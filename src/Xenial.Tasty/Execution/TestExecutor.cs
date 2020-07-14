using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Commands;
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
                .UseTestGroupForceVisitor()
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
            var testQueue = await ExecuteTestsCommand.Execute(this);
            testQueue = await ExecuteTestsCommand.VisitForcedTestCases(testQueue);
            await ExecuteTestsCommand.Execute(this, testQueue);
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

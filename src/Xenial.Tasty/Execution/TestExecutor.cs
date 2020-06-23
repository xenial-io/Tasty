using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
            async Task ExecuteTests(IExecutable[] executableItems)
            {
                for (var i = 0; i < executableItems.Length; i++)
                {
                    var executable = executableItems[i];
                    if (executable is TestGroup group)
                    {
                        Scope.CurrentGroup = group;
                        var sw = Stopwatch.StartNew();
                        var groupResult = await executable.Executor.Invoke();
                        if (groupResult)
                        {
                            foreach (var action in group.Executors)
                            {
                                if (action is TestCase testCase)
                                {
                                    await Execute(testCase);
                                }
                                if (action is TestGroup testGroup)
                                {
                                    await ExecuteTests(new[] { testGroup });
                                }
                            }
                        }
                        sw.Stop();
                        group.Duration = sw.Elapsed;
                    }
                    if (executable is TestCase test)
                    {
                        await Execute(test);
                    }
                }
            }

            ForceTestVisitor.MarkTestsAsForced(Scope);

            await ExecuteTests(Scope.Executors.ToArray());
        }

        internal async Task Execute(TestCase testCase)
        {
            var app = Build();
            var context = new TestExecutionContext(testCase, Scope, testCase.Group);
            await app(context);
        }

        internal TestDelegate Build()
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
    }
}

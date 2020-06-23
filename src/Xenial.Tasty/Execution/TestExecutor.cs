using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution.Middleware;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Scopes;
using Xenial.Delicious.Visitors;

namespace Xenial.Delicious.Execution
{
    public class TestExecutor
    {
        private IList<Func<TestDelegate, TestDelegate>> Middlewares = new List<Func<TestDelegate, TestDelegate>>();
        internal TastyScope Scope { get; }
        
        public TestExecutor(TastyScope scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));

            this
                .UseTestReporters()
                .UseStopwatch()
                .UseForcedTestExecutor()
                .UseIgnoreTestExecutor()
                .UseBeforeEachTest()
                .UseTestExecutor()
                .UseAfterEachTest()
                ;
        }

        public TestExecutor Use(Func<TestDelegate, TestDelegate> middleware)
        {
            Middlewares.Add(middleware);
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
            var context = new TestContext(testCase, Scope, testCase.Group);
            await app(context);
        }

        internal TestDelegate Build()
        {
            TestDelegate app = context =>
            {
                return Task.CompletedTask;
            };

            foreach (var middleware in Middlewares.Reverse())
            {
                app = middleware(app);
            }

            return app;
        }
    }
}

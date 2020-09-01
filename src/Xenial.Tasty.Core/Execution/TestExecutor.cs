using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Execution.TestGroupMiddleware;
using Xenial.Delicious.Execution.TestMiddleware;
using Xenial.Delicious.Execution.TestRuntime;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Execution
{
    public class TestExecutor
    {
        private readonly IList<Func<TestDelegate, TestDelegate>> testMiddlewares = new List<Func<TestDelegate, TestDelegate>>();
        private readonly IList<Func<TestGroupDelegate, TestGroupDelegate>> testGroupMiddlewares = new List<Func<TestGroupDelegate, TestGroupDelegate>>();
        private readonly IList<Func<RuntimeDelegate, RuntimeDelegate>> runtimeMiddlewares = new List<Func<RuntimeDelegate, RuntimeDelegate>>();
        internal TastyScope Scope { get; }

        public TestExecutor(TastyScope scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));

            this
                .UseRemoteDisposal()
                .UseEndTestPipeline()
                .UseTestPipelineCompleted()
                .UseResetConsoleColor()
                .UseExitCodeReporter()
                .UseSummaryReporters()
                .UseInteractiveRunDetection()
                .UseParseConnectionString()
                .UseRemote()
                .UseRegisterCommands()
                .UseSelectCommand()
                .UseClearConsole()
                .UseRemoteClearConsole()
                .UseResetRemoteConsoleColor()
                .UseRunCommands()
                ;

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
            testMiddlewares.Add(middleware);
            return this;
        }

        public TestExecutor Use(Func<TestGroupDelegate, TestGroupDelegate> middleware)
        {
            testGroupMiddlewares.Add(middleware);
            return this;
        }

        public TestExecutor Use(Func<RuntimeDelegate, RuntimeDelegate> middleware)
        {
            runtimeMiddlewares.Add(middleware);
            return this;
        }

        public async Task<int> Execute()
        {
            using var context = new RuntimeContext(this);

            var endPipeline = context.EndPipeLine;
            while (!endPipeline)
            {
                var app = BuildRuntimeMiddleware();

                await app(context).ConfigureAwait(false);
                endPipeline = context.EndPipeLine;
            }

            return context.ExitCode;
        }

        internal RuntimeDelegate BuildRuntimeMiddleware()
        {
            RuntimeDelegate app = context =>
            {
                return Task.CompletedTask;
            };

            foreach (var middleware in runtimeMiddlewares.Reverse())
            {
                app = middleware(app);
            }

            return app;
        }

        internal TestDelegate BuildTestMiddleware()
        {
            TestDelegate app = context =>
            {
                return Task.CompletedTask;
            };

            foreach (var middleware in testMiddlewares.Reverse())
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

            foreach (var middleware in testGroupMiddlewares.Reverse())
            {
                app = middleware(app);
            }

            return app;
        }
    }
}

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
        private readonly IList<Func<TestDelegate, TestDelegate>> TestMiddlewares = new List<Func<TestDelegate, TestDelegate>>();
        private readonly IList<Func<TestGroupDelegate, TestGroupDelegate>> TestGroupMiddlewares = new List<Func<TestGroupDelegate, TestGroupDelegate>>();
        private readonly IList<Func<RuntimeDelegate, RuntimeDelegate>> RuntimeMiddlewares = new List<Func<RuntimeDelegate, RuntimeDelegate>>();
        internal TastyScope Scope { get; }

        public TestExecutor(TastyScope scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));

            this
                .UseEndTestPipeline()
                .UseTestPipelineCompleted()
                .UseResetConsoleColor()
                .UseRemoteDisposal()
                .UseExitCodeReporter()
                .UseSummaryReporters()
                .UseInteractiveRunDetection()
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
            TestMiddlewares.Add(middleware);
            return this;
        }

        public TestExecutor Use(Func<TestGroupDelegate, TestGroupDelegate> middleware)
        {
            TestGroupMiddlewares.Add(middleware);
            return this;
        }

        public TestExecutor Use(Func<RuntimeDelegate, RuntimeDelegate> middleware)
        {
            RuntimeMiddlewares.Add(middleware);
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

            foreach (var middleware in RuntimeMiddlewares.Reverse())
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

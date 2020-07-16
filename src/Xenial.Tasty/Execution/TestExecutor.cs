using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Commands;
using Xenial.Delicious.Execution.TestGroupMiddleware;
using Xenial.Delicious.Execution.TestMiddleware;
using Xenial.Delicious.Execution.TestRuntime;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Execution
{
    public class TestExecutor
    {
        private IList<Func<TestDelegate, TestDelegate>> TestMiddlewares = new List<Func<TestDelegate, TestDelegate>>();
        private IList<Func<TestGroupDelegate, TestGroupDelegate>> TestGroupMiddlewares = new List<Func<TestGroupDelegate, TestGroupDelegate>>();
        private IList<Func<RuntimeDelegate, RuntimeDelegate>> RuntimeMiddlewares = new List<Func<RuntimeDelegate, RuntimeDelegate>>();
        internal TastyScope Scope { get; }

        public TestExecutor(TastyScope scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));

            this
                .UseClearConsole()
                .UseEndTestPipelinePipeline()
                .UseTestPipelineCompleted()
                .UseResetConsoleColor()
                .UseRemoteDisposal()
                .UseExitCodeReporter()
                .UseSummaryReporters()
                .UseInteractiveRunDetection()
                .UseRemote()
                .UseRegisterCommands()
                .UseRemoteClearConsole()
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
            using var context = new RuntimeContext(Scope, this);

            while (!context.EndPipeLine)
            {
                var app = BuildRuntimeMiddleware();

                await app(context);
            }

            //var testQueue = await ExecuteTestsCommand.Execute(this);
            //testQueue = await ExecuteTestsCommand.VisitForcedTestCases(testQueue);
            //await ExecuteTestsCommand.Execute(this, testQueue);
            return context.ExitCode;
        }

        //public Task<bool> WaitForCommand()
        //{
        //    if (Remote != null)
        //    {
        //        var tcs = new TaskCompletionSource<bool>();

        //        async void ExecuteCommand(object _, Protocols.ExecuteCommandEventArgs e)
        //        {
        //            await Execute();
        //            tcs.SetResult(true);
        //        }
        //        void CancellationRequested(object _, EventArgs __)
        //        {
        //            tcs.SetResult(false);
        //        }
        //        void Exit(object _, EventArgs __)
        //        {
        //            tcs.SetCanceled();
        //        }
        //        Remote.ExecuteCommand -= ExecuteCommand;
        //        Remote.ExecuteCommand += ExecuteCommand;
        //        Remote.CancellationRequested -= CancellationRequested;
        //        Remote.CancellationRequested += CancellationRequested;
        //        Remote.Exit -= Exit;
        //        Remote.Exit += Exit;

        //        return tcs.Task;
        //    }
        //    return Task.FromResult(false);
        //}

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

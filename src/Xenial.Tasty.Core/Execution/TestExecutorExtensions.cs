using System;
using System.Linq;
using System.Threading.Tasks;

namespace Xenial.Delicious.Execution
{
    public static class TestExecutorExtensions
    {
        public static TestExecutor Use(this TestExecutor app, Func<TestExecutionContext, Func<Task>, Task> middleware)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));

            return app.Use(next =>
            {
                return context =>
                {
                    Func<Task> simpleNext = () => next(context);
                    return middleware(context, simpleNext);
                };
            });
        }

        public static TestExecutor UseGroup(this TestExecutor app, Func<TestGroupContext, Func<Task>, Task> middleware)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));

            return app.Use(next =>
            {
                return context =>
                {
                    Func<Task> simpleNext = () => next(context);
                    return middleware(context, simpleNext);
                };
            });
        }

        public static TestExecutor UseRuntime(this TestExecutor app, Func<RuntimeContext, Func<Task>, Task> middleware)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));

            return app.Use(next =>
            {
                return context =>
                {
                    Func<Task> simpleNext = () => next(context);
                    return middleware(context, simpleNext);
                };
            });
        }
    }
}

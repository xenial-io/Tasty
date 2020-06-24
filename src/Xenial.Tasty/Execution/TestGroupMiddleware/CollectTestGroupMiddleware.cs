using System;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Execution.TestGroupMiddleware
{
    public static class CollectTestGroupMiddleware
    {
        public static TestExecutor UseTestGroupCollector(this TestExecutor executor)
            => executor.UseGroup(async (context, next) =>
            {
                foreach (var testGroup in context.CurrentGroup.Executors.OfType<TestGroup>())
                {
                    if (!context.GroupQueue.Contains(testGroup))
                    {
                        context.GroupQueue.Enqueue(testGroup);
                    }
                }

                await next();
            });

    }
}

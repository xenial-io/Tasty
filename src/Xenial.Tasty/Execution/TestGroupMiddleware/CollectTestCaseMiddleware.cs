using System;
using System.Linq;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Execution.TestGroupMiddleware
{
    public static class CollectTestCaseMiddleware
    {
        public static TestExecutor UseTestCaseCollector(this TestExecutor executor)
           => executor.UseGroup(async (context, next) =>
           {
               foreach (var testCase in context.CurrentGroup.Executors.OfType<TestCase>())
               {
                   if (!context.TestQueue.Contains(testCase))
                   {
                       context.TestQueue.Enqueue(testCase);
                   }
               }

               await next();
           });
    }
}

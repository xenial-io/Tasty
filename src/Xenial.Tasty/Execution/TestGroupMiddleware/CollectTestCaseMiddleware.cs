using System;
using System.Linq;

using Xenial.Delicious.Metadata;
using Xenial.Delicious.Visitors;

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
        public static TestExecutor UseTestGroupForceVisitor(this TestExecutor executor)
            => executor.UseGroup(async (context, next) =>
            {
                try
                {
                    if (context.CurrentGroup.IsForced != null)
                    {
                        var result = context.CurrentGroup.IsForced();

                        foreach (var child in context.CurrentGroup.Descendants().OfType<IForceAble>())
                        {
                            child.IsForced = () => result;
                        }
                    }
                    await next();
                }
                catch (Exception ex)
                {
                    context.CurrentGroup.Exception = ex;
                    context.CurrentGroup.TestOutcome = TestOutcome.Failed;
                }
            });
    }
}

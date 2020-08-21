using System.Collections.Generic;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Reporters
{
    /// <summary>
    /// Delegate AsyncTestReporter
    /// Gets called immediate after the test was executed
    /// <see cref="Xenial.Tasty.RegisterReporter(AsyncTestReporter)"/>
    /// </summary>
    /// <param name="test">The test.</param>
    /// <returns>Task.</returns>
    public delegate Task AsyncTestReporter(TestCaseResult test);

    /// <summary>
    /// Delegate AsyncTestSummaryReporter
    /// Gets called after all tests are executed.
    /// <see cref="Xenial.Tasty.RegisterReporter(AsyncTestSummaryReporter)"/>
    /// </summary>
    /// <param name="tests">The tests.</param>
    /// <returns>Task.</returns>
    public delegate Task AsyncTestSummaryReporter(IEnumerable<TestCaseResult> tests);
}

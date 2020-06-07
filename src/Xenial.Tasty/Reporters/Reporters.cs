using System.Collections.Generic;
using System.Threading.Tasks;

using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Reporters
{
    public delegate Task AsyncTestReporter(TestCase test);

    public delegate Task AsyncTestSummaryReporter(IEnumerable<TestCase> tests);
}

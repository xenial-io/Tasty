using System;
using System.Threading.Tasks;

using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.DeclarativeTests
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var scope = new TastyScope()
                .RegisterReporter(ConsoleReporter.Report)
                .RegisterReporter(ConsoleReporter.ReportSummary);

            var group = scope.Describe("I'm a group", () => { });

            group.It("with an test case", () => true);

            return await scope.Run(args);
        }
    }
}

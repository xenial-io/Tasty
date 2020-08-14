using System;
using System.Linq;

namespace Xenial.Delicious.Metadata
{
    public enum TestOutcome
    {
        NotRun = 0,
        Ignored = 99,
        Failed = 999,
        Success = 9999,
    }
}

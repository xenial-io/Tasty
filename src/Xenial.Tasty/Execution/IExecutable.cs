using System;
using System.Threading.Tasks;

namespace Xenial.Delicious.Execution
{
    internal interface IExecutable
    {
        Func<Task<bool>> Executor { get; }
    }
}
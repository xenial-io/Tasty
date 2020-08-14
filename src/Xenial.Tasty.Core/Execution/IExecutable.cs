using System;
using System.Threading.Tasks;

namespace Xenial.Delicious.Execution
{
    internal interface IExecutable
    {
        Executable Executor { get; }
    }
}
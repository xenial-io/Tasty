using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Xenial.Tasty.Tool
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            var interactiveCommand = new Command("interactive")
            {
                Handler = CommandHandler.Create(() => Interactive())
            };
            interactiveCommand.AddAlias("i");
            rootCommand.Add(interactiveCommand);

            return await rootCommand.InvokeAsync(args);
        }

        static Task<int> Interactive()
        {
            return Task.FromResult(0);
        }
    }
}

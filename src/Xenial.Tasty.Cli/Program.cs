using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Cli.Commands;

namespace Xenial.Delicious.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            var interactiveCommand = new Command("interactive")
            {
                Handler = CommandHandler.Create<string, CancellationToken>(InteractiveCommand.Interactive)
            };
            interactiveCommand.AddAlias("i");
            var arg = new Option<string>("--project");
            arg.AddAlias("-p");
            interactiveCommand.Add(arg);
            rootCommand.Add(interactiveCommand);

            var studioCommand = new Command("studio")
            {
                Handler = CommandHandler.Create<CancellationToken>(StudioCommand.Studio)
            };
            studioCommand.AddAlias("s");
            rootCommand.Add(studioCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}

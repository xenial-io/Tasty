using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Cli.Commands;

namespace Xenial.Delicious.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            SetupConsoleEncoding();

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
                Handler = CommandHandler.Create(StudioCommand.Studio)
            };
            studioCommand.AddAlias("s");
            rootCommand.Add(studioCommand);

            return await rootCommand.InvokeAsync(args);
        }

        private static void SetupConsoleEncoding()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //Register additional code pages for windows
                //cause we deal directly with process streams
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
    }
}

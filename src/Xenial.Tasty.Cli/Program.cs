using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

using static SimpleExec.Command;

namespace Xenial.Tasty.Tool
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            var interactiveCommand = new Command("interactive")
            {
                Handler = CommandHandler.Create<string>(Interactive)
            };
            interactiveCommand.AddAlias("i");
            var arg = new Option<string>("--project");
            arg.AddAlias("-p");
            interactiveCommand.Add(arg);
            rootCommand.Add(interactiveCommand);

            return await rootCommand.InvokeAsync(args);
        }

        static async Task<int> Interactive(string project)
        {
            var path = Path.GetFullPath(project);
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                var directoryName = new DirectoryInfo(path).Name;
                var csProjFileName = Path.Combine(path, $"{directoryName}.csproj");
                if (File.Exists(csProjFileName))
                {
                    Console.WriteLine(csProjFileName);
                    await RunAsync("dotnet", $"run -p \"{csProjFileName}\" -f netcoreapp3.1",
                        configureEnvironment: (env) =>
                        {
                            env.Add("TASTY_INTERACTIVE", "true");
                        });
                }
            }
            return 0;
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using static SimpleExec.Command;

namespace Tasty.Build
{
    static partial class Program
    {
        static (string fullFramework, string netcore, string net5) FindTfms()
        {
            var dirProps = XElement.Load("Directory.Build.props");
            var props = dirProps.Descendants("PropertyGroup");
            var fullFramework = props.Descendants("FullFrameworkVersion").First().Value;
            var netcore = props.Descendants("NetCoreVersion").First().Value;
            var net5 = props.Descendants("Net5Version").First().Value;
            return (fullFramework, netcore, net5);
        }

        async static Task EnsureTools()
        {
            try
            {
                await RunAsync("dotnet", "format --version");
            }
            catch (SimpleExec.NonZeroExitCodeException)
            {
                //Can't find dotnet format, assuming tools are not installed
                await RunAsync("dotnet", "tool restore");
            }
        }

        static string Tabify(string s)
            => string.Join(
                Environment.NewLine, 
                s.Split("\n").Select(s => $"\t{s}")
            );
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using static SimpleExec.Command;

namespace Tasty.Build
{
    static partial class Program
    {

    }
    static partial class Program
    {
        static (string fullFramework, string netcore) FindTfms()
        {
            var dirProps = XElement.Load("Directory.Build.props");
            var props = dirProps.Descendants("PropertyGroup");
            var fullFramework = props.Descendants("FullFrameworkVersion").First().Value;
            var netcore = props.Descendants("NetCoreVersion").First().Value;
            return (fullFramework, netcore);
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

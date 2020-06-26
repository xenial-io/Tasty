using System;
using System.Linq;
using System.Xml.Linq;

namespace Tasty.Build
{
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
    }
}
